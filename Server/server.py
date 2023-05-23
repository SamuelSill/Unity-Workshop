from typing import Optional
from fastapi import FastAPI, Response, status

from pymongo.database import Database
from pymongo.mongo_client import MongoClient
from pymongo.collection import Collection
from pymongo.server_api import ServerApi

from pydantic import BaseModel

# region Resources

mongo_db: Database = MongoClient(
    "mongodb+srv://DrawNDrive:3Q5Vnhvwdt7fwKGe@drawndrive.nvnngxr.mongodb.net/?retryWrites=true&w=majority",
    server_api=ServerApi('1')
).get_database("DrawNDrive")

players_collection: Collection = mongo_db.get_collection("players")
cars_collection: Collection = mongo_db.get_collection("cars")

app = FastAPI()

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
        "achievements": [],
        "friends": [],
        "money": 1000,
        "sum_accruacy": 1000,
        "cars": [
            {
                "id": "auto",
                "upgrades": {}
            }
        ],
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
    return player_found["gallery"]


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
                "data": new_painting.data,
                "description": new_painting.description,
                "difficulty": generated_difficulty
            }
        }
    })

    response.status_code = status.HTTP_200_OK

    return generated_difficulty

# endregion


# region Player Cars

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

    player_found["money"] -= car_found["price"]

    player_found["cars"].append({
        "id": car_id,
        "upgrades": {
            upgrade["id"]: 0
            for upgrade in car_found["upgrades"]
        }
    })

    players_collection.update_one({"username": username}, {
        "$push": {
            "cars": {
                "id": car_id,
                "upgrades": {
                    upgrade["id"]: 0
                    for upgrade in car_found["upgrades"]
                }
            }
        }
    })

    response.status_code = status.HTTP_200_OK
    return "Car Purchased"


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

    if upgrade_pricing := next((car_upgrade["pricing"]
                                for car_upgrade in car_found["upgrades"]
                                if car_upgrade["id"] == upgrade_id),
                               __default=None) is None:
        return "Upgrade Not Found!"

    if owned_car := next((owned_car
                          for owned_car in player_found["cars"]
                          if owned_car["id"] == car_id),
                         __default=None):
        response.status_code = status.HTTP_409_CONFLICT
        return "Given Player Doesn't Own the Car!"

    if owned_car["upgrades"][upgrade_id] == len(upgrade_pricing):
        response.status_code = status.HTTP_409_CONFLICT
        return "Car Already Upgraded to the Max!"

    if player_found["money"] < upgrade_pricing[owned_car["upgrades"][upgrade_id]]:
        response.status_code = status.HTTP_409_CONFLICT
        return "Upgrade too Expensive!"

    players_collection.update_one({"username": username}, {
        "$inc": {
            "money": -upgrade_pricing[owned_car["upgrades"][upgrade_id]],
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

    if skin_price := next((car_skin["price"]
                           for car_skin in car_found["skins"]
                           if car_skin["id"] == skin_id),
                          __default=None) is None:
        return "Skin Not Found!"

    if owned_car := next((owned_car
                          for owned_car in player_found["cars"]
                          if owned_car["id"] == car_id),
                         __default=None) is None:
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


@app.post("/players/games",
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

# endregion


# region Main

def run_server():
    from uvicorn import run
    run(app, port=9583)


if __name__ == '__main__':
    run_server()

# endregion
