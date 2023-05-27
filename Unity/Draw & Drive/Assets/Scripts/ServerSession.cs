using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class ServerSession : MonoBehaviour
{
    // User Details
    private static string _loggedUsername = "";
    private static string _loggedPassword = "";
    private static int _userMoney = 0;
    private static List<string> _userFriends = new();

    // User Paintings
    private static List<Painting> _userPaintings = new();

    // User Cars
    private static List<PlayerCar> _ownedCars = new List<PlayerCar>();
    private static int _selectedCarIndex = 0;

    // All Cars
    private static List<Car> _cars = new();

    //  Consts
    private const string serverURL = "localhost:5555";
    private static string credentialsFile = "";

    // Properties
    public static string Username => _loggedUsername;
    public static int Money => _userMoney;
    public static List<string> Friends => _userFriends;
    public static List<Painting> Paintings => _userPaintings;
    public static List<PlayerCar> OwnedCars => _ownedCars;
    public static PlayerCar CurrentCar => _ownedCars[_selectedCarIndex];
    public static int CurrentCarIndex => _selectedCarIndex;
    public static List<Car> GameCars => _cars;
    public static Car CurrentGameCar => GameCars.Find(car => car.id == CurrentCar.id);
    public static string CurrentSkin => CurrentCar.skins[CurrentCar.selected_skin];
    public static int SpeedUpgradeCost => CurrentCar.upgrades.speed == CurrentGameCar.upgrades.speed.Count ? -1 : CurrentGameCar.upgrades.speed[CurrentCar.upgrades.speed];
    public static int ThicknessUpgradeCost => CurrentCar.upgrades.thickness == CurrentGameCar.upgrades.thickness.Count ? -1 : CurrentGameCar.upgrades.thickness[CurrentCar.upgrades.thickness];
    public static int SteeringUpgradeCost => CurrentCar.upgrades.steering == CurrentGameCar.upgrades.steering.Count ? -1 : CurrentGameCar.upgrades.steering[CurrentCar.upgrades.steering];
    public static int CurrentCarSpeed => CurrentGameCar.speed + CurrentCar.upgrades.speed;
    public static int CurrentCarThickness => CurrentGameCar.thickness + CurrentCar.upgrades.thickness;
    public static int CurrentCarSteering => CurrentGameCar.steering + CurrentCar.upgrades.steering;

    private static ServerSession session;

    void Awake()
    {
        session = this;
        credentialsFile = Application.persistentDataPath + "/credentials.dat";
    }

    public static void CreateUser(
        string username, 
        string password, 
        string firstName, 
        string lastName,
        Action createUserSuccessfulCallback,
        Action createUserFailedCallback
    )
    {
        session.StartCoroutine(session.TryCreateUser(username, password, firstName, lastName, createUserSuccessfulCallback, createUserFailedCallback));
    }

    IEnumerator TryCreateUser(
        string username, 
        string password, 
        string firstName, 
        string lastName, 
        Action createUserSuccessfulCallback, 
        Action createUserFailedCallback
    )
    {
        UnityWebRequest postRequest = new UnityWebRequest($"{serverURL}/players/register", "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(
            $"{{ " +
            $"\"username\": \"{username}\", " +
            $"\"password\": \"{password}\", " +
            $"\"first_name\": \"{firstName}\", " +
            $"\"last_name\": \"{lastName}\"" +
            $"}}"
        );

        postRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
        postRequest.downloadHandler = new DownloadHandlerBuffer();
        postRequest.SetRequestHeader("Content-Type", "application/json");

        //Send the request then wait here until it returns
        yield return postRequest.SendWebRequest();

        if (postRequest.result == UnityWebRequest.Result.ConnectionError || postRequest.responseCode != 200)
        {
            createUserFailedCallback.Invoke();
        }
        else
        {
            createUserSuccessfulCallback.Invoke();
        }
    }

    public static void SelectCar(int newCarIndex, Action carSelectedSuccessfully)
    {
        if (newCarIndex < _ownedCars.Count && newCarIndex >= 0)
        {
            session.StartCoroutine(session.SelectCarIndex(newCarIndex, carSelectedSuccessfully));
        }
    }

    IEnumerator SelectCarIndex(int newCarIndex, Action carSelectedSuccessfully)
    {
        UnityWebRequest getRequest = UnityWebRequest.Put($"{serverURL}/players/cars?username={_loggedUsername}&password={_loggedPassword}&car_index={newCarIndex}", "");
        yield return getRequest.SendWebRequest();

        if (getRequest.result != UnityWebRequest.Result.ConnectionError && getRequest.responseCode == 200)
        {
            _selectedCarIndex = newCarIndex;
            carSelectedSuccessfully.Invoke();
        }
    }

    public static void SelectSkin(int newSkinIndex, Action skinSelectedSuccessfully)
    {
        if (newSkinIndex < CurrentCar.skins.Count && newSkinIndex >= 0)
        {
            session.StartCoroutine(session.SelectSkinIndex(newSkinIndex, skinSelectedSuccessfully));
        }
    }

    IEnumerator SelectSkinIndex(int newSkinIndex, Action skinSelectedSuccessfully)
    {
        UnityWebRequest getRequest = UnityWebRequest.Put($"{serverURL}/players/cars/skins?username={_loggedUsername}&password={_loggedPassword}&car_index={_selectedCarIndex}&skin_index={newSkinIndex}", "");
        yield return getRequest.SendWebRequest();

        if (getRequest.result != UnityWebRequest.Result.ConnectionError && getRequest.responseCode == 200)
        {
            _ownedCars[_selectedCarIndex].selected_skin = newSkinIndex;
            skinSelectedSuccessfully.Invoke();
        }
    }

    public static void PurchaseCar(string carID, Action carBoughtSuccessfully)
    {
        Car carToBuy = _cars.Find(car => car.id == carID);
        if (_userMoney >= carToBuy.price)
        {
            session.StartCoroutine(session.BuyCar(carToBuy, carBoughtSuccessfully));
        }
    }

    IEnumerator BuyCar(Car carToBuy, Action carBoughtSuccessfully)
    {
        var uwr = UnityWebRequest.Post($"{serverURL}/players/cars?username={_loggedUsername}&password={_loggedPassword}&car_id={carToBuy.id}", "");

        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.ConnectionError && uwr.responseCode == 200)
        {
            _userMoney -= carToBuy.price;
            _ownedCars.Add(new PlayerCar
            {
                id = carToBuy.id,
                selected_skin = 0,
                skins = new List<string> { carToBuy.skins[0].id },
                upgrades = new PlayerCarUpgrade {
                    speed = 0, 
                    steering = 0, 
                    thickness = 0 
                }
            });

            carBoughtSuccessfully.Invoke();
        }
    }

    public static void PurchaseUpgrade(string upgradeID, Action upgradeBoughtSuccessfully)
    {
        int price = upgradeID == "speed" ? SpeedUpgradeCost : upgradeID == "thickness" ? ThicknessUpgradeCost : SteeringUpgradeCost;
        if (_userMoney >= price)
        {
            session.StartCoroutine(session.BuyUpgrade(upgradeID, price, upgradeBoughtSuccessfully));
        }
    }

    IEnumerator BuyUpgrade(string upgradeID, int price, Action upgradeBoughtSuccessfully)
    {
        var uwr = UnityWebRequest.Put($"{serverURL}/players/cars/upgrades?username={_loggedUsername}&password={_loggedPassword}&car_id={CurrentGameCar.id}&upgrade_id={upgradeID}", "");

        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.ConnectionError && uwr.responseCode == 200)
        {
            _userMoney -= price;

            switch (upgradeID)
            {
                case "speed":
                    CurrentCar.upgrades.speed += 1;
                    break;
                case "thickness":
                    CurrentCar.upgrades.thickness += 1;
                    break;
                case "steering":
                    CurrentCar.upgrades.steering += 1;
                    break;
            }

            upgradeBoughtSuccessfully.Invoke();
        }
    }

    public static void PurchaseSkin(string skinID, Action skinBoughtSuccessfully)
    {
        Skin skin = CurrentGameCar.skins.Find(skin => skin.id == skinID);
        
        if (_userMoney >= skin.price)
        {
            session.StartCoroutine(session.BuySkin(skin, skinBoughtSuccessfully));
        }
    }

    IEnumerator BuySkin(Skin skin, Action skinBoughtSuccessfully)
    {
        var uwr = UnityWebRequest.Post($"{serverURL}/players/cars/skins?username={_loggedUsername}&password={_loggedPassword}&car_id={CurrentGameCar.id}&skin_id={skin.id}", "");

        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.ConnectionError && uwr.responseCode == 200)
        {
            _userMoney -= skin.price;
            CurrentCar.skins.Add(skin.id);

            skinBoughtSuccessfully.Invoke();
        }
    }

    public static void UploadPainting(string name, string description, string pngFilePath, Action<Painting> paintingAdded)
    {
        session.StartCoroutine(session.AddPainting(name, description, pngFilePath, paintingAdded));
    }

    [System.Serializable]
    public class AddNewPainting
    {
        public string name;
        public List<int> data;
        public string description;
    }

    IEnumerator AddPainting(string paintingName, string description, string fileLocation, Action<Painting> paintingAdded)
    {
        if (File.Exists(fileLocation))
        {
            byte[] data = File.ReadAllBytes(fileLocation);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(data);

            AddNewPainting painting = new AddNewPainting();
            painting.name = paintingName;
            painting.description = description;
            painting.data = new List<int>();
            foreach (byte b in data)
            {
                painting.data.Add(b);
            }

            var uwr = new UnityWebRequest($"{serverURL}/players/paintings?username={_loggedUsername}&password={_loggedPassword}", "POST");
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(JsonUtility.ToJson(painting));

            uwr.uploadHandler = new UploadHandlerRaw(jsonToSend);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.SetRequestHeader("Content-Type", "application/json");

            //Send the request then wait here until it returns
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.ConnectionError && uwr.responseCode == 200)
            {
                Painting newPainting = new()
                {
                    description = description,
                    name = paintingName,
                    difficulty = float.Parse(System.Text.Encoding.UTF8.GetString(uwr.downloadHandler.data)),
                    pngData = texture
                };

                _userPaintings.Add(newPainting);
                paintingAdded.Invoke(newPainting);
            }
        }
    }

    public static void AddNewFriend(string friendUsername, Action friendAddedSuccessfully)
    {
        session.StartCoroutine(session.AddFriend(friendUsername, friendAddedSuccessfully));
    }

    IEnumerator AddFriend(string friendUsername, Action friendAddedSuccessfully)
    {
        UnityWebRequest getRequest = UnityWebRequest.Post($"{serverURL}/players/friends?username={_loggedUsername}&password={_loggedPassword}&friend_username={friendUsername}", "");
        yield return getRequest.SendWebRequest();

        if (getRequest.result != UnityWebRequest.Result.ConnectionError && getRequest.responseCode == 200)
        {
            _userFriends.Add(friendUsername);
            friendAddedSuccessfully.Invoke();
        }
    }

    public static void DeleteFriend(string friendUsername, Action friendDeletedSuccessfully)
    {
        session.StartCoroutine(session.DeleteExistingFriend(friendUsername, friendDeletedSuccessfully));
    }

    IEnumerator DeleteExistingFriend(string friendUsername, Action friendDeletedSuccessfully)
    {
        UnityWebRequest getRequest = UnityWebRequest.Delete($"{serverURL}/players/friends?username={_loggedUsername}&password={_loggedPassword}&friend_username={friendUsername}");
        yield return getRequest.SendWebRequest();

        if (getRequest.result != UnityWebRequest.Result.ConnectionError && getRequest.responseCode == 200)
        {
            friendDeletedSuccessfully.Invoke();
        }
    }

    public static void LogOut()
    {
        if (File.Exists(credentialsFile)) File.Delete(credentialsFile);

        // User Details
        _loggedUsername = "";
        _loggedPassword = "";
        _userMoney = 0;
        _userFriends.Clear();

        // User Paintings
        _userPaintings.Clear();

        // User Cars
        _ownedCars.Clear();
        _selectedCarIndex = 0;
        _cars.Clear();
    }

    public static void Initialize(Action loginSuccessfulCallback, Action loginFailedCallback)
    {
        FileStream file;

        if (File.Exists(credentialsFile))
        {
            file = File.OpenRead(credentialsFile);
            BinaryFormatter bf = new BinaryFormatter();
            UserCredentials credentials = (UserCredentials)bf.Deserialize(file);
            file.Close();

            session.StartCoroutine(session.TryLogin(credentials.username, credentials.password, false, loginSuccessfulCallback, loginFailedCallback));
        }
    }

    public static void Initialize(string username, string password, bool rememberUser, Action loginSuccessfulCallback, Action loginFailedCallback)
    {
        session.StartCoroutine(session.TryLogin(username, password, rememberUser, loginSuccessfulCallback, loginFailedCallback));
    }

    [System.Serializable]
    public class UserCredentials
    {
        public UserCredentials(string username, string password)
        {
            this.username = username;
            this.password = password;
        }

        public string username;
        public string password;
    }

    IEnumerator TryLogin(string username, string password, bool rememberUser, Action loginSuccessfulCallback, Action loginFailedCallback)
    {
        UnityWebRequest getRequest = UnityWebRequest.Get($"{serverURL}/players/login?username={username}&password={password}");
        yield return getRequest.SendWebRequest();

        if (getRequest.result == UnityWebRequest.Result.ConnectionError)
        {
            if (File.Exists(credentialsFile))
            {
                File.Delete(credentialsFile);
            }

            loginFailedCallback.Invoke();
        }
        else if (getRequest.responseCode != 200)
        {
            loginFailedCallback.Invoke();
        }
        else
        {
            _loggedUsername = username;
            _loggedPassword = password;

            if (rememberUser)
            {
                FileStream file;

                if (File.Exists(credentialsFile)) file = File.OpenWrite(credentialsFile);
                else file = File.Create(credentialsFile);

                UserCredentials data = new UserCredentials(username, password);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(file, data);
                file.Close();
            }

            StartCoroutine(LoadUserData(loginSuccessfulCallback));
        }
    }
    IEnumerator LoadUserData(Action loginSuccessfulCallback)
    {
        yield return GetUserMoney();
        yield return GetUserPaintings();
        yield return GetFriends();
        yield return GetUserOwnedCars();
        yield return GetUserSelectedCar();
        yield return GetAllCars();

        loginSuccessfulCallback.Invoke();
    }

    IEnumerator GetUserMoney()
    {
        UnityWebRequest getRequest = UnityWebRequest.Get($"{serverURL}/players/stats/money?username={_loggedUsername}&password={_loggedPassword}");
        yield return getRequest.SendWebRequest();

        if (getRequest.result != UnityWebRequest.Result.ConnectionError && getRequest.responseCode == 200)
        {
            _userMoney = int.Parse(getRequest.downloadHandler.text);
        }
    }

    [System.Serializable]
    public class JsonPainting
    {
        public string name;
        public List<int> data;
        public string description;
        public float difficulty;
    }

    [System.Serializable]
    class JsonPaintings
    {
        public JsonPainting[] paintings;
    }

    public class Painting
    {
        public string name;
        public string description;
        public float difficulty;
        public Texture2D pngData;
    }

    IEnumerator GetUserPaintings()
    {
        UnityWebRequest getRequest = UnityWebRequest.Get($"{serverURL}/players/paintings?username={_loggedUsername}&password={_loggedPassword}");
        yield return getRequest.SendWebRequest();

        if (getRequest.result != UnityWebRequest.Result.ConnectionError && getRequest.responseCode == 200)
        {
            var paintings = JsonUtility.FromJson<JsonPaintings>($"{{\"paintings\": {getRequest.downloadHandler.text}}}");

            foreach (var painting in paintings.paintings)
            {
                Texture2D texture = new Texture2D(2, 2);
                byte[] data = new byte[painting.data.Count];
                for (int index = 0; index < data.Length; index++)
                {
                    data[index] = (byte)painting.data[index];
                }

                texture.LoadImage(data);
                _userPaintings.Add(new Painting
                {
                    name = painting.name,
                    description = painting.description,
                    difficulty = painting.difficulty,
                    pngData = texture
                });
            }
        }
    }

    [System.Serializable]
    class JsonFriends
    {
        public List<string> names;
    }

    IEnumerator GetFriends()
    {
        UnityWebRequest getRequest = UnityWebRequest.Get($"{serverURL}/players/friends?username={_loggedUsername}&password={_loggedPassword}");
        yield return getRequest.SendWebRequest();

        if (getRequest.result != UnityWebRequest.Result.ConnectionError && getRequest.responseCode == 200)
        {
            _userFriends = JsonUtility.FromJson<JsonFriends>($"{{\"names\": {getRequest.downloadHandler.text}}}").names;
        }
    }

    [System.Serializable]
    public class PlayerCarUpgrade
    {
        public int speed;
        public int thickness;
        public int steering;
    }

    [System.Serializable]
    public class PlayerCar
    {
        public string id;
        public PlayerCarUpgrade upgrades;
        public List<string> skins;
        public int selected_skin;
    }

    [System.Serializable]
    class PlayerCars
    {
        public List<PlayerCar> cars;
    }

    IEnumerator GetUserOwnedCars()
    {
        UnityWebRequest getRequest = UnityWebRequest.Get($"{serverURL}/players/cars?username={_loggedUsername}&password={_loggedPassword}");
        yield return getRequest.SendWebRequest();

        if (getRequest.result != UnityWebRequest.Result.ConnectionError && getRequest.responseCode == 200)
        {
            _ownedCars = JsonUtility.FromJson<PlayerCars>($"{{\"cars\": {getRequest.downloadHandler.text}}}").cars;
        }
    }

    [System.Serializable]
    class SelectedCar
    {
        public int selectedCarIndex;
    }

    IEnumerator GetUserSelectedCar()
    {
        UnityWebRequest selectedCarRequest = UnityWebRequest.Get($"{serverURL}/players/cars/selected?username={_loggedUsername}&password={_loggedPassword}");
        yield return selectedCarRequest.SendWebRequest();

        if (selectedCarRequest.result != UnityWebRequest.Result.ConnectionError && selectedCarRequest.responseCode == 200)
        {
            _selectedCarIndex = JsonUtility.FromJson<SelectedCar>($"{{\"selectedCar\": {selectedCarRequest.downloadHandler.text}}}").selectedCarIndex;
        }
    }

    [System.Serializable]
    public class UpgradePricings
    {
        public List<int> speed;
        public List<int> steering;
        public List<int> thickness;
    }

    [System.Serializable]
    public class Skin
    {
        public string id;
        public int price;
    }

    [System.Serializable]
    public class Car
    {
        public string id;
        public string description;
        public int speed;
        public int thickness;
        public int steering;
        public UpgradePricings upgrades;
        public List<Skin> skins;
        public int price;
    }

    [System.Serializable]
    public class Cars
    {
        public List<Car> cars;
    }

    IEnumerator GetAllCars()
    {
        var getRequest = UnityWebRequest.Get($"{serverURL}/cars");

        //Send the request then wait here until it returns
        yield return getRequest.SendWebRequest();

        if (getRequest.result != UnityWebRequest.Result.ConnectionError && getRequest.responseCode == 200)
        {
            _cars = JsonUtility.FromJson<Cars>($"{{\"cars\": {getRequest.downloadHandler.text}}}").cars;
        }
    }
}
