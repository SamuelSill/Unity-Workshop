import asyncio
from typing import Optional
from fastapi import FastAPI, Response, status, WebSocket, WebSocketDisconnect
from fastapi.responses import HTMLResponse

from pymongo.database import Database
from pymongo.mongo_client import MongoClient
from pymongo.collection import Collection
from pymongo.server_api import ServerApi
from fastapi.middleware.cors import CORSMiddleware

import gridfs
from pydantic import BaseModel

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


def generate_difficulty(_: list[int]) -> float:
    from random import random
    # TODO: Difficulty Algorithm and Machine Learning

    return random()

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
        "money": 1000,
        "sum_accuracy": 0,
        "games_won": 0,
        "games_lost": 0,
        "cars": [
            {
                "id": "auto",
                "upgrades": {},
                "skins": ["regular"],
                "selected_skin": 0
            }
        ],
        "selected_car": 0,
        "gallery": []
    })

    return "Success"


@app.get("/players/login",
         status_code=status.HTTP_404_NOT_FOUND)
def get_login(username: str,
              password: str,
              response: Response):
    if player(username, password) is None:
        return "Player Not Found!"

    response.status_code = status.HTTP_200_OK
    return "Success"


@app.get("/games/gyro_page")
def get_gyro_page():
    with open("gyro_html.html", "r") as gyro_html_file:
        return HTMLResponse(gyro_html_file.read())


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


class NewPainting(BaseModel):
    name: str
    data: list[int]
    description: str


@app.post("/players/paintings",
          status_code=status.HTTP_404_NOT_FOUND)
def post_paintings(username: str,
                   password: str,
                   new_painting: NewPainting,
                   response: Response):
    if player(username, password) is None:
        return "Player Not Found!"

    generated_difficulty: float = generate_difficulty(new_painting.data)
    players_collection.update_one({"username": username}, {
        "$push": {
            "gallery": {
                "name": new_painting.name,
                "data": grid_fs.put(bytes(new_painting.data)),
                "description": new_painting.description,
                "difficulty": generated_difficulty
            }
        }
    })

    response.status_code = status.HTTP_200_OK

    return generated_difficulty

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

    if (owned_car := next((owned_car
                           for owned_car in player_found["cars"]
                           if owned_car["id"] == car_id),
                          None)) is None:
        response.status_code = status.HTTP_409_CONFLICT
        return "Given Player Doesn't Own the Car!"

    if owned_car["upgrades"][upgrade_id] == len(car_found["upgrades"][upgrade_id]):
        response.status_code = status.HTTP_409_CONFLICT
        return "Car Already Upgraded to the Max!"

    if player_found["money"] < car_found["upgrades"][upgrade_id][owned_car["upgrades"][upgrade_id]]:
        response.status_code = status.HTTP_409_CONFLICT
        return "Upgrade too Expensive!"

    players_collection.update_one({"username": username}, {
        "$inc": {
            "money": -car_found["upgrades"][upgrade_id][owned_car["upgrades"][upgrade_id]],
            f"upgrades.{upgrade_id}": 1
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
                 password: str,
                 new_game: NewGame,
                 response: Response):
    if player(username, password) is None:
        return "Player Not Found!"

    response.status_code = status.HTTP_200_OK

    players_collection.update_one({"username": username}, {
        "$inc": {
            ("games_won" if new_game.win else "games_lost"): 1,
            "sum_accuracy": new_game.accuracy
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
    def __init__(self,
                 username: str,
                 game_host: WebSocket):
        self.__host: str = username
        self.__player_sockets: dict[str, WebSocket] = {username: game_host}

    @property
    def __host_socket(self) -> WebSocket:
        return self.__player_sockets[self.__host]

    async def join(self,
                   username: str,
                   password: str,
                   websocket: WebSocket) -> bool:
        if (player_found := player(username, password)) is None:
            return False

        if username in self.__player_sockets:
            return False

        await asyncio.gather(*[
            player_socket.send_json({
                "id": "UserJoined",
                "username": username,
                "selected_car": player_found["cars"][player_found["selected_car"]]
            })
            for player_socket in self.__player_sockets.values()
        ])

        self.__player_sockets[username] = websocket
        await websocket.send_json({
            "id": "GameJoined",
            "players": [
                {
                    "username": player_username,
                    "selected_car": players_collection.find_one({
                        "username": player_username
                    })["cars"][player_found["selected_car"]]
                }
                for player_username in self.__player_sockets
                if player_username != username
            ]
        })

        return True

    async def leave(self,
                    username: str,
                    password: str) -> bool:
        if player(username, password) is None:
            return False

        if username not in self.__player_sockets:
            return False

        self.__player_sockets.pop(username)
        await asyncio.gather(*[
            player_socket.send_json({
                "id": "UserLeft",
                "username": username,
            })
            for player_socket in self.__player_sockets.values()
        ])

        return True

    async def start(self,
                    other_game: "Game") -> None:
        from random import choice

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
                "host_ip": self.__host_socket.client.host,
                "is_host": username == self.__host,
                "painting": random_player_painting,
                "team": "left" if username in self.__player_sockets else "right"
            })
            for username, player_socket in players.items()
        ])

    async def close(self) -> None:
        await asyncio.gather(*[
            player_socket.send_json({"id": "GameClosed"})
            for player_socket in self.__player_sockets.values()
        ])
        self.__player_sockets = {}

    def __len__(self) -> int:
        return len(self.__player_sockets)


games: dict[str, Game] = {}
searching_game_code: str = ""


@app.websocket("/games/ws/{username}/{password}")
async def create_game(websocket: WebSocket,
                      username: str,
                      password: str):
    from string import ascii_uppercase
    from random import choice

    global searching_game_code

    await websocket.accept()
    if (player_found := player(username, password)) is None:
        await websocket.send_json({
            "id": "ErrorCreating",
            "message": "Player Not Found!"
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

    games[game_code] = Game(username, websocket)
    await websocket.send_json({
        "id": "GameCreated",
        "code": game_code
    })

    try:
        while True:
            data: dict[str, str] = await websocket.receive_json()
            if data["id"] == "StartGame":
                # if len(games[game_code]) < 3:
                #     await websocket.send_json({
                #          "id": "ErrorStarting",
                #          "message": "Not Enough Players!"
                #      })
                if searching_game_code == "":
                    searching_game_code = game_code
                else:
                    await games[searching_game_code].start(games[game_code])
                    searching_game_code = ""
    except WebSocketDisconnect:
        pass

    if searching_game_code == game_code:
        searching_game_code = ""

    await games[game_code].leave(username, password)
    await games[game_code].close()
    games.pop(game_code)


@app.websocket("/games/ws/{game_code}/{username}/{password}")
async def join_game(websocket: WebSocket,
                    game_code: str,
                    username: str,
                    password: str):
    await websocket.accept()
    if (player_found := player(username, password)) is None:
        await websocket.send_json({
            "id": "ErrorJoining",
            "message": "Player Not Found!"
        })
        await websocket.close()
        return

    #if len(player_found["gallery"]) == 0:
        #print("Error:    Can't Play without Paintings!")
        #await websocket.send_json({
        #    "id": "ErrorCreating",
        #    "message": "Can't Play without Paintings!"
        # })
        # await websocket.close()

        #return

    if game_code not in games:
        await websocket.send_json({
            "id": "ErrorJoining",
            "message": "Game Not Found!"
        })
        await websocket.close()
        return

    if len(games[game_code]) == 3:
        await websocket.send_json({
            "id": "ErrorJoining",
            "message": "Game Full!"
        })
        await websocket.close()
        return

    await games[game_code].join(username, password, websocket)

    try:
        while True:
            await websocket.receive_text()
    except WebSocketDisconnect:
        pass

    if game_code in games:
        await games[game_code].leave(username, password)

# endregion


# region Main

def run_server():
    from uvicorn import run
    run(app, host="0.0.0.0", port=80)


if __name__ == '__main__':
    run_server()

# endregion
