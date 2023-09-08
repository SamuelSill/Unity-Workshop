import asyncio
from typing import Optional
from fastapi import FastAPI, Response, status, WebSocket, WebSocketDisconnect
from starlette.websockets import WebSocketState
from fastapi.responses import RedirectResponse, PlainTextResponse, FileResponse

from pymongo.database import Database
from pymongo.mongo_client import MongoClient
from pymongo.collection import Collection
from pymongo.server_api import ServerApi
from fastapi.middleware.cors import CORSMiddleware

import gridfs
from pydantic import BaseModel
import logging
import logging.config

import math
import numpy as np
from skimage.segmentation import slic
from skimage.color import label2rgb
import matplotlib.image as mpimg
from PIL import Image

# region Logging

logging.config.fileConfig("logging.conf")
logger: logging.Logger = logging.getLogger()

# endregion

# region Resources

mongo_db: Database = MongoClient(
    "mongodb+srv://DrawNDrive:3Q5Vnhvwdt7fwKGe@drawndrive.nvnngxr.mongodb.net/?retryWrites=true&w=majority",
    server_api=ServerApi('1')
).get_database("DrawNDrive")

grid_fs: gridfs.GridFS = gridfs.GridFS(mongo_db)

players_collection: Collection = mongo_db.get_collection("players")
cars_collection: Collection = mongo_db.get_collection("cars")

app = FastAPI()


# allow CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"]
)

# endregion

# region Google Cloud Healthcheck


@app.get("/health_check")
def health_check():
    return


# endregion


# region Frontend

@app.get("/app/{frontend_page:path}", name="path-convertor")
def mobile_app_page(frontend_page: str):
    return FileResponse(frontend_page)


# endregion


# region Utility Functions

def player(username: str,
           password: str) -> Optional[dict[str, ...]]:
    return players_collection.find_one({
        "username": username,
        "password": password
    })


def car(car_id: str) -> Optional[dict[str, ...]]:
    return cars_collection.find_one({
        "id": car_id
    })


# endregion


# region Login & Registration


class RegistrationForm(BaseModel):
    username: str
    password: str
    first_name: str
    last_name: str
    description: Optional[str]


@app.post("/players/register", status_code=status.HTTP_409_CONFLICT)
def post_register(registration_form: RegistrationForm,
                  response: Response):
    if players_collection.find_one({
        "username": registration_form.username
    }) is not None:
        return "User Already Exists!"

    response.status_code = status.HTTP_200_OK

    players_collection.insert_one({
        "username": registration_form.username,
        "password": registration_form.password,
        "first_name": registration_form.first_name,
        "last_name": registration_form.last_name,
        "description": registration_form.description,
        "achievements": [],
        "friends": [],
        "money": 500,
        "sum_accuracy": 0,
        "games_won": 0,
        "games_lost": 0,
        "cars": [
            {
                "id": "auto",
                "upgrades": {
                    "speed": 0,
                    "steering": 0,
                    "thickness": 0
                },
                "skins": ["regular"],
                "selected_skin": 0
            }
        ],
        "selected_car": 0,
        "gallery": []
    })

    return "Success"


@app.get("/")
async def home_page():
    return RedirectResponse("/app/mobile/index.html")


@app.get("/games")
async def get_running_games():
    return PlainTextResponse(
        "No games"
        if len(games) == 0
        else
        "\n\n".join((f"{game_code}\n"
                     f"Game Running: {game.is_running}\n" +
                     "\n".join(game))
                    for game_code, game in games.items())
    )


@app.get("/players/login",
         status_code=status.HTTP_404_NOT_FOUND)
def get_login(username: str,
              password: str,
              response: Response):
    if player(username, password) is None:
        return "Player Not Found!"

    response.status_code = status.HTTP_200_OK
    return "Success"


# endregion


# region Player Statistics

@app.get("/players/stats/money",
         status_code=status.HTTP_404_NOT_FOUND)
def get_money(username: str,
              password: str,
              response: Response):
    if (player_found := player(username, password)) is None:
        return "Player Not Found!"

    response.status_code = status.HTTP_200_OK
    return player_found["money"]


@app.get("/players/stats",
         status_code=status.HTTP_404_NOT_FOUND)
def get_stats(username: str,
              response: Response):
    if (player_found := players_collection.find_one({
        "username": username
    })) is None:
        return "Player Not Found!"

    response.status_code = status.HTTP_200_OK
    return {
        "username": username,
        "description": player_found["description"],
        "games_lost": player_found["games_lost"],
        "games_won": player_found["games_won"],
        "sum_accuracy": player_found["sum_accuracy"],
        "selected_car": player_found["cars"][player_found["selected_car"]]
    }

# endregion


# region Paintings


@app.get("/players/paintings",
         status_code=status.HTTP_404_NOT_FOUND)
def get_paintings(username: str,
                  password: str,
                  response: Response):
    if (player_found := player(username, password)) is None:
        return "Player Not Found!"

    response.status_code = status.HTTP_200_OK
    return [
        {
            **painting,
            "data": list(grid_fs.get(painting["data"]).read())
        }
        for painting in player_found["gallery"]
    ]


def process_image(painting_data: bytes, shape: list[int], file_type: str) -> bytes:
    from random import choice
    from string import ascii_lowercase
    from os import remove
    image_filename: str = "".join(choice(ascii_lowercase) for _ in range(10)) + "." + file_type

    with open(image_filename, "wb") as f:
        f.write(painting_data)

    image_decoded_array = np.frombuffer(mpimg.imread(image_filename).tobytes(), dtype=np.uint8)
    image_reshaped = image_decoded_array.reshape((*shape, 3))

    # Applying Simple Linear Iterative
    # Clustering on the image
    # - 50 segments & compactness = 10
    length: int = shape[0] + shape[1]
    segments = slic(image_reshaped, n_segments=20 * 4048 / length, compactness=2 * 4048 / length)

    # Converts a label image into
    # an RGB color image for visualizing
    # the labeled regions.
    divided_img = label2rgb(segments, image_reshaped, kind='avg')

    spectrum_size: int = 3

    # Convert all to simple colors
    for image_row in range(divided_img.shape[0]):
        for image_column in range(divided_img.shape[1]):
            pixel_rgb = divided_img[image_row][image_column]

            for m in range(3):
                divided_img[image_row][image_column][m] = (
                    math.floor(pixel_rgb[m] / (255 / spectrum_size)) *
                    (255 / (spectrum_size - 1))
                )

            r, g, b = pixel_rgb

            # Convert black to blue and white to yellow
            if r == g == b == 0:
                divided_img[image_row][image_column][2] = 255
            if r == g == b == 127:
                divided_img[image_row][image_column][0] = 0
                divided_img[image_row][image_column][1] = 127
                divided_img[image_row][image_column][2] = 0
            if r == g == b == 255:
                divided_img[image_row][image_column][2] = 0

    Image.fromarray(divided_img, "RGB").save(image_filename)
    with open(image_filename, "rb") as f:
        data: bytes = f.read()
    remove(image_filename)
    return data


class NewPainting(BaseModel):
    name: str
    data: list[int]
    shape: list[int]
    description: str
    fileType: str


@app.delete("/players/paintings",
            status_code=status.HTTP_404_NOT_FOUND)
def delete_paintings(username: str,
                     password: str,
                     painting: str,
                     response: Response):
    if (player_found := player(username, password)) is None:
        return "Player Not Found!"

    if (painting_found := next((user_painting
                                for user_painting in player_found["gallery"]
                                if user_painting["name"] == painting),
                               None)) is None:
        return "Painting Not Found!"

    grid_fs.delete(painting_found["data"])

    players_collection.update_one({"username": username}, {
        "$pull": {
            "gallery": {"name": painting}
        }
    })

    response.status_code = status.HTTP_200_OK

    return "Painting Deleted Successfully"


@app.post("/players/paintings",
          status_code=status.HTTP_404_NOT_FOUND)
def post_paintings(username: str,
                   password: str,
                   new_painting: NewPainting,
                   response: Response):
    if (player_found := player(username, password)) is None:
        return "Player Not Found!"

    if any(painting["name"] == new_painting.name for painting in player_found["gallery"]):
        return "Painting Already Exists!"

    processed_image: bytes = process_image(bytes(new_painting.data), new_painting.shape, new_painting.fileType)
    painting: dict = {
        "name": new_painting.name,
        "data": grid_fs.put(processed_image),
        "description": new_painting.description
    }

    players_collection.update_one({"username": username}, {
        "$push": {
            "gallery": painting
        }
    })

    response.status_code = status.HTTP_200_OK

    return {
        **painting,
        "data": list(processed_image)
    }

# endregion


# region Player Cars

@app.get("/cars")
def get_game_cars():
    result: list[dict[str, ...]] = []
    for game_car in cars_collection.find():
        result_car: dict[str, ...] = {**game_car}
        result_car.pop("_id")
        result.append(result_car)
    return result


@app.get("/players/cars",
         status_code=status.HTTP_404_NOT_FOUND)
def get_player_cars(username: str,
                    password: str,
                    response: Response):
    if (player_found := player(username, password)) is None:
        return "Player Not Found!"

    response.status_code = status.HTTP_200_OK
    return player_found["cars"]


@app.post("/players/cars",
          status_code=status.HTTP_404_NOT_FOUND)
def buy_car(username: str,
            password: str,
            car_id: str,
            response: Response):
    if (player_found := player(username, password)) is None:
        return "Player Not Found!"

    if (car_found := car(car_id)) is None:
        return "Car Not Found"

    if any(owned_car["id"] == car_id for owned_car in player_found["cars"]):
        response.status_code = status.HTTP_409_CONFLICT
        return "Car Already Owned!"

    if player_found["money"] < car_found["price"]:
        response.status_code = status.HTTP_409_CONFLICT
        return "Car too Expensive!"

    players_collection.update_one({"username": username}, {
        "$push": {
            "cars": {
                "id": car_id,
                "upgrades": {
                    upgrade: 0
                    for upgrade in car_found["upgrades"]
                },
                "skins": [
                    car_found["skins"][0]["id"]
                ],
                "selected_skin": 0
            }
        },
        "$inc": {
            "money": -car_found["price"]
        }
    })

    response.status_code = status.HTTP_200_OK
    return "Car Purchased"


@app.put("/players/cars",
         status_code=status.HTTP_404_NOT_FOUND)
def select_car(username: str,
               password: str,
               car_index: int,
               response: Response):
    if (player_found := player(username, password)) is None:
        return "Player Not Found!"

    if car_index < 0 or car_index >= len(player_found["cars"]):
        response.status_code = status.HTTP_409_CONFLICT
        return "Invalid Car Index!"

    players_collection.update_one({"username": username}, {
        "$set": {
            "selected_car": car_index
        }
    })

    response.status_code = status.HTTP_200_OK
    return "Car Selected"


@app.get("/players/cars/selected",
         status_code=status.HTTP_404_NOT_FOUND)
def get_selected_car(username: str,
                     password: str,
                     response: Response):
    if (player_found := player(username, password)) is None:
        return "Player Not Found!"

    response.status_code = status.HTTP_200_OK
    return player_found["selected_car"]


@app.put("/players/cars/skins",
         status_code=status.HTTP_404_NOT_FOUND)
def select_car_skin(username: str,
                    password: str,
                    car_index: int,
                    skin_index: int,
                    response: Response):
    if (player_found := player(username, password)) is None:
        return "Player Not Found!"

    if car_index < 0 or car_index >= len(player_found["cars"]):
        response.status_code = status.HTTP_409_CONFLICT
        return "Invalid Car Index!"

    if skin_index < 0 or skin_index >= len(player_found["cars"][car_index]["skins"]):
        response.status_code = status.HTTP_409_CONFLICT
        return "Invalid Skin Index!"

    players_collection.update_one({"username": username}, {
        "$set": {
            f"cars.{car_index}.selected_skin": skin_index
        }
    })

    response.status_code = status.HTTP_200_OK
    return "Skin Selected"


@app.put("/players/cars/upgrades",
         status_code=status.HTTP_404_NOT_FOUND)
def upgrade_car(username: str,
                password: str,
                car_id: str,
                upgrade_id: str,
                response: Response):
    if (player_found := player(username, password)) is None:
        return "Player Not Found!"

    if (car_found := car(car_id)) is None:
        return "Car Not Found!"

    if upgrade_id not in ("speed", "steering", "thickness"):
        return "Upgrade Not Found!"

    if (owned_car_index := next((car_index
                                 for car_index in range(len(player_found["cars"]))
                                 if player_found["cars"][car_index]["id"] == car_id),
                                None)) is None:
        response.status_code = status.HTTP_409_CONFLICT
        return "Given Player Doesn't Own the Car!"

    owned_car: dict[str, ...] = player_found["cars"][owned_car_index]

    if owned_car["upgrades"][upgrade_id] == len(car_found["upgrades"][upgrade_id]):
        response.status_code = status.HTTP_409_CONFLICT
        return "Car Already Upgraded to the Max!"

    if player_found["money"] < car_found["upgrades"][upgrade_id][owned_car["upgrades"][upgrade_id]]:
        response.status_code = status.HTTP_409_CONFLICT
        return "Upgrade too Expensive!"

    players_collection.update_one({"username": username}, {
        "$inc": {
            "money": -car_found["upgrades"][upgrade_id][owned_car["upgrades"][upgrade_id]],
            f"cars.{owned_car_index}.upgrades.{upgrade_id}": 1
        }
    })

    response.status_code = status.HTTP_200_OK
    return "Upgrade Purchased"


@app.post("/players/cars/skins",
          status_code=status.HTTP_404_NOT_FOUND)
def buy_car_skin(username: str,
                 password: str,
                 car_id: str,
                 skin_id: str,
                 response: Response):
    if (player_found := player(username, password)) is None:
        return "Player Not Found!"

    if (car_found := car(car_id)) is None:
        return "Car Not Found!"

    if (skin_price := next((car_skin["price"]
                            for car_skin in car_found["skins"]
                            if car_skin["id"] == skin_id),
                           None)) is None:
        return "Skin Not Found!"

    if (owned_car := next((owned_car
                           for owned_car in player_found["cars"]
                           if owned_car["id"] == car_id),
                          None)) is None:
        response.status_code = status.HTTP_409_CONFLICT
        return "Given Player Doesn't Own the Car!"

    if any(owned_skin == skin_id for owned_skin in owned_car["skins"]):
        response.status_code = status.HTTP_409_CONFLICT
        return "Car Skin Already Purchased!"

    if player_found["money"] < skin_price:
        response.status_code = status.HTTP_409_CONFLICT
        return "Skin too Expensive!"

    players_collection.update_one({"username": username}, {
        "$inc": {
            "money": -skin_price
        },
        "$push": {
            f"cars.{player_found['cars'].index(owned_car)}.skins": skin_id
        }
    })

    response.status_code = status.HTTP_200_OK
    return "Upgrade Purchased"

# endregion


# region Player Friends


@app.get("/players/friends",
         status_code=status.HTTP_404_NOT_FOUND)
def get_friends(username: str,
                password: str,
                response: Response):
    if (player_found := player(username, password)) is None:
        return "Player Not Found!"

    response.status_code = status.HTTP_200_OK

    return player_found["friends"]


@app.post("/players/friends",
          status_code=status.HTTP_404_NOT_FOUND)
def add_friend(username: str,
               password: str,
               friend_username: str,
               response: Response):
    if (player_found := player(username, password)) is None:
        return "Player Not Found!"

    if players_collection.find_one({"username": friend_username}) is None:
        return "Friend Username Not Found"

    if any(friend == friend_username for friend in player_found["friends"]):
        response.status_code = status.HTTP_409_CONFLICT
        return "Players are Already Friends!"

    players_collection.update_one({"username": username}, {
        "$push": {"friends": friend_username}
    })

    response.status_code = status.HTTP_200_OK
    return "Added Friend"


@app.delete("/players/friends",
            status_code=status.HTTP_404_NOT_FOUND)
def remove_friend(username: str,
                  password: str,
                  friend_username: str,
                  response: Response):
    if (player_found := player(username, password)) is None:
        return "Player Not Found!"

    if players_collection.find_one({"username": friend_username}) is None:
        return "Friend Username Not Found"

    if all(friend != friend_username for friend in player_found["friends"]):
        response.status_code = status.HTTP_409_CONFLICT
        return "Players are not Friends!"

    players_collection.update_one({"username": username}, {
        "$pull": {"friends": friend_username}
    })

    response.status_code = status.HTTP_200_OK
    return "Removed Friend"


# endregion


# region Player Games & Achievements

class NewGame(BaseModel):
    win: bool
    accuracy: float


@app.post("/players/games",
          status_code=status.HTTP_404_NOT_FOUND)
def add_new_game(username: str,
                 new_game: NewGame,
                 response: Response):
    if players_collection.find_one({"username": username}) is None:
        return "Player Not Found!"

    response.status_code = status.HTTP_200_OK

    players_collection.update_one({"username": username}, {
        "$inc": {
            ("games_won" if new_game.win else "games_lost"): 1,
            "sum_accuracy": new_game.accuracy,
            "money": 100 if new_game.win else 50
        }
    })

    return "New Game Saved"


@app.get("/players/achievements",
         status_code=status.HTTP_404_NOT_FOUND)
def get_achievements(username: str,
                     password: str,
                     response: Response):
    if (player_found := player(username, password)) is None:
        return "Player Not Found!"

    response.status_code = status.HTTP_200_OK

    return player_found["achievements"]


@app.post("/players/achievements",
          status_code=status.HTTP_404_NOT_FOUND)
def add_achievement(username: str,
                    password: str,
                    achievement_id: str,
                    response: Response):
    from time import strftime
    if player(username, password) is None:
        return "Player Not Found!"

    response.status_code = status.HTTP_200_OK

    players_collection.update_one({"username": username}, {
        "$push": {
            "achievements": {"id": achievement_id, "time": strftime("%d/%m/%Y %H:%M:%S")}
        }
    })

    return "Achievement Saved"


class Game:
    class LoggingLock:
        def __init__(self):
            from asyncio import Lock
            self.__lock: Lock = Lock()
            self.print = True

        async def __aenter__(self):
            if self.print:
                logger.debug("Acquiring lock")
            await self.__lock.acquire()

        async def __aexit__(self, exc_type, exc_val, exc_tb):
            if self.print:
                logger.debug("Releasing lock")
            self.__lock.release()

    def __init__(self,
                 username: str,
                 game_host: WebSocket):
        from typing import Optional

        self.__host: str = username
        self.__game_lock: Game.LoggingLock = Game.LoggingLock()
        self.__unity_host: Optional[WebSocket] = None
        self.__player_sockets: dict[str, WebSocket] = {username: game_host}
        self.__mobile_players: set[str] = set()
        self.__other_game: Optional[Game] = None
        self.__friends_only: bool = False

    def __contains__(self,
                     username: str) -> bool:
        return username in self.__player_sockets

    def __iter__(self):
        for username in self.__player_sockets.keys():
            yield username

    async def join(self,
                   username: str,
                   password: str,
                   websocket: WebSocket,
                   mobile: bool) -> bool:
        async with self.__game_lock:
            player_found: dict[str, ...] = player(username, password)

            if self.__friends_only and all(username != friend for friend in player_found["friends"]):
                return False

            if username in self.__player_sockets:
                return False

            await asyncio.gather(*[
                player_socket.send_json({
                    "id": "UserJoined",
                    "username": username,
                    "selected_car": player_found["cars"][player_found["selected_car"]],
                    "mobile": mobile
                })
                for player_socket in self.__player_sockets.values()
            ])

            await websocket.send_json({
                "id": "GameJoined",
                "players": [
                    {
                        "username": player_username,
                        "selected_car": (other_player := players_collection.find_one({
                            "username": player_username
                        }))["cars"][other_player["selected_car"]],
                        "mobile": player_username in self.__mobile_players
                    }
                    for player_username in self.__player_sockets
                ]
            })

            self.__player_sockets[username] = websocket

            if mobile:
                self.__mobile_players.add(username)

            return True

    async def leave(self,
                    username: str,
                    password: str) -> bool:
        async with self.__game_lock:
            if player(username, password) is None:
                return False

            if username not in self.__player_sockets:
                return False

            self.__player_sockets.pop(username)

            if username in self.__mobile_players:
                self.__mobile_players.remove(username)

            await asyncio.gather(*[
                player_socket.send_json({
                    "id": "UserLeft",
                    "username": username,
                })
                for player_socket in self.__player_sockets.values()
            ])

            return True

    async def start(self,
                    other_game: "Game",
                    host_ip: str) -> None:
        from random import choice

        async with self.__game_lock:
            players: dict[str, WebSocket] = {**self.__player_sockets, **other_game.__player_sockets}

            random_player_painting: list[int] = list(grid_fs.get(choice([
                painting["data"]
                for player_object in players_collection.find({
                    "username": {"$in": list(players.keys())}
                })
                for painting in player_object["gallery"]
            ])).read())

            await asyncio.gather(*[
                player_socket.send_json({
                    "id": "GameStarted",
                    "host_ip": host_ip,
                    "is_host": username == self.__host,
                    "painting": random_player_painting,
                    "team": "left" if username in self.__player_sockets else "right",
                    "enemy_lobby": [
                        {
                            "username": enemy_username,
                            "selected_car":
                                (enemy_player := players_collection.find_one({
                                    "username": enemy_username
                                }))["cars"][enemy_player["selected_car"]],
                            "mobile": enemy_username in other_game.__mobile_players
                        }
                        for enemy_username in players
                        if (enemy_username in self) != (username in self)
                    ]
                })
                for username, player_socket in players.items()
            ])

            self.__other_game = other_game
            other_game.__other_game = self

            self.__other_game.__unity_host = self.__unity_host = self.__player_sockets[self.__host]

    async def stop(self) -> None:
        async with self.__game_lock:
            if self.is_running:
                self.__other_game.__other_game = None
                self.__other_game.__unity_host = None
                self.__other_game = None
                self.__unity_host = None

    async def close(self) -> None:
        async with self.__game_lock:
            await asyncio.gather(*[
                player_socket.send_json({"id": "GameClosed"})
                for player_socket in self.__player_sockets.values()
                if (
                    player_socket.application_state == WebSocketState.CONNECTED and
                    player_socket.client_state == WebSocketState.CONNECTED
                )
            ])

            await asyncio.gather(*[
                player_socket.close()
                for player_socket in self.__player_sockets.values()
                if (
                    player_socket.application_state == WebSocketState.CONNECTED and
                    player_socket.client_state == WebSocketState.CONNECTED
                )
            ])

            self.__player_sockets = {}
            self.__mobile_players = set()

    def __len__(self) -> int:
        return len(self.__player_sockets)

    async def send_mobile_controls(self,
                                   mobile_controls: dict[str, ...]) -> None:
        self.__game_lock.print = False
        try:
            async with self.__game_lock:
                if (
                    self.__unity_host.application_state == WebSocketState.CONNECTED and
                    self.__unity_host.client_state == WebSocketState.CONNECTED
                ):
                    await self.__unity_host.send_json(mobile_controls)
        finally:
            self.__game_lock.print = True

    @property
    def is_running(self) -> bool:
        return self.__other_game is not None

    async def set_friends_only(self,
                               activate: bool) -> None:
        async with self.__game_lock:
            self.__friends_only = activate


games: dict[str, Game] = {}
searching_game_code: str = ""
searching_game_host: str = ""


@app.websocket("/games/ws/{username}/{password}")
async def create_game(websocket: WebSocket,
                      username: str,
                      password: str):
    from string import ascii_uppercase
    from random import choice

    global searching_game_code
    global searching_game_host

    logger.debug(f"Connection Opened (Create): {username}")

    await websocket.accept()

    if (player_found := player(username, password)) is None:
        logger.error(f"{username}:{password} are not valid credentials!")
        await websocket.send_json({
            "id": "ErrorCreating",
            "message": "Player Not Found!"
        })
        await websocket.close()

        return

    if any(username in game for game in games.values()):
        logger.error(f"{username} has tried to create a game, yet they are already in one!")
        await websocket.send_json({
            "id": "ErrorCreating",
            "message": "Player Already in a Game!"
        })
        await websocket.close()

        return

    # if len(player_found["gallery"]) == 0:
    #     await websocket.send_json({
    #         "id": "ErrorCreating",
    #         "message": "Can't Play without Paintings!"
    #     })
    #     await websocket.close()
    #
    #     return

    while (game_code := ''.join(choice(ascii_uppercase) for _ in range(4))) in games:
        pass

    logger.info(f"{username} created a new game with code: {game_code}")

    games[game_code] = Game(username, websocket)
    await websocket.send_json({
        "id": "GameCreated",
        "code": game_code
    })

    try:
        while True:
            data: dict[str, ...] = await websocket.receive_json()
            if data["id"] == "FriendsOnly":
                logger.info(f"{username} has updated friends only setting in game {game_code}")
                await games[game_code].set_friends_only(data["activate"] == "True")
            elif data["id"] == "StartGame":
                logger.info(f"{username} started matching ({game_code})...")
                # if len(games[game_code]) < 3:
                #     await websocket.send_json({
                #          "id": "ErrorStarting",
                #          "message": "Not Enough Players!"
                #      })
                if searching_game_code == "":
                    logger.info(f"{username}'s game ({game_code}) is waiting...")
                    searching_game_code = game_code
                    searching_game_host = data["ip"]
                elif searching_game_code != game_code:
                    logger.info(f"{username}'s game ({game_code}) has matched with searching game: {searching_game_code}...")
                    await games[searching_game_code].start(games[game_code], searching_game_host)
                    searching_game_code = ""
                    searching_game_host = ""
            if data["id"] == "FinishGame":
                logger.info(f"{username}'s game has finished ({game_code})")
                await games[game_code].stop()
    except WebSocketDisconnect:
        logger.info(f"{username} has disconnected, game {game_code} is closing...")
        pass

    if searching_game_code == game_code:
        logger.debug(f"{game_code} was waiting for other games before closing...")
        searching_game_code = ""
        searching_game_host = ""

    await games[game_code].leave(username, password)
    await games[game_code].close()
    games.pop(game_code)


@app.websocket("/games/ws/{game_code}/{username}/{password}/{mobile}")
async def join_game(websocket: WebSocket,
                    game_code: str,
                    username: str,
                    password: str,
                    mobile: str):
    logger.debug(f"Connection Opened (Join): {username}")

    await websocket.accept()

    if (player_found := player(username, password)) is None:
        logger.error(f"{username}:{password} are not valid credentials!")
        await websocket.send_json({
            "id": "ErrorJoining",
            "message": "Player Not Found!"
        })
        await websocket.close()
        return

    if any(username in game for game in games.values()):
        logger.error(f"{username} has tried to join a game, yet they are already in one!")
        await websocket.send_json({
            "id": "ErrorJoining",
            "message": "Player Already in a Game!"
        })
        await websocket.close()

        return

    #if len(player_found["gallery"]) == 0:
        #await websocket.send_json({
        #    "id": "ErrorJoining",
        #    "message": "Can't Play without Paintings!"
        # })
        # await websocket.close()

        #return

    if game_code not in games:
        logger.error(f"{username} has tried to join a game that does not exist: {game_code}")
        await websocket.send_json({
            "id": "ErrorJoining",
            "message": "Game Not Found!"
        })
        await websocket.close()
        return

    if len(games[game_code]) == 3:
        logger.error(f"{username} has tried to join a full game: {game_code}")
        await websocket.send_json({
            "id": "ErrorJoining",
            "message": "Game Full!"
        })
        await websocket.close()
        return

    mobile: bool = mobile.lower() == "true"

    if not await games[game_code].join(username, password, websocket, mobile):
        logger.error(f"{username} has failed to join the game: {game_code}")
        await websocket.send_json({
            "id": "ErrorJoining",
            "message": "Failed Joining Game!"
        })
        await websocket.close()
        return

    logger.info(f"{username} joined the game with the code: {game_code}")

    if mobile:
        logger.debug(f"{username} has connected from a mobile device")

    try:
        mobile_controls_log_count: int = 0
        while (websocket.application_state == WebSocketState.CONNECTED and
               websocket.client_state == WebSocketState.CONNECTED):
            mobile_controls: dict[str, ...] = await websocket.receive_json()
            mobile_controls["username"] = username
            if mobile and game_code in games and games[game_code].is_running:
                if mobile_controls_log_count % 20 == 0:
                    logger.debug(f"{username}'s Mobile Controls: {mobile_controls}")
                mobile_controls_log_count += 1
                await games[game_code].send_mobile_controls(mobile_controls)
    except WebSocketDisconnect:
        logger.info(f"{username} has disconnected from game {game_code}")
        pass

    if game_code in games:
        logger.info(f"{username} is leaving game {game_code}")
        await games[game_code].leave(username, password)
    else:
        logger.warning(f"{username} is leaving nonexistent game {game_code}")

# endregion


# region Main

def run_server():
    from uvicorn import run
    run(app, host="0.0.0.0", port=80)


if __name__ == '__main__':
    run_server()

# endregion
