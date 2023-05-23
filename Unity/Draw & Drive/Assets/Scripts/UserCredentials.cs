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
