using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

    public class Data : MonoBehaviour
    {
        public static void SaveProfile(ProfileData t_profile)
        {
            try
            {
                string path = Application.persistentDataPath + "/profile.dt";

                if (File.Exists(path)) File.Delete(path);

                FileStream file = File.Create(path);

                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(file, t_profile);
                file.Close();

                Debug.Log("SAVED SUCCESSFULY");
            }
            catch
            {
                Debug.Log("SOMETHING WENT TERRIBLY WRONG");
            }
        }

        public static ProfileData LoadProfile()
        {
            ProfileData ret = new ProfileData();

            try
            {
                string path = Application.persistentDataPath + "/profile.dt";

                if (File.Exists(path))
                {
                    FileStream file = File.Open(path, FileMode.Open);
                    BinaryFormatter bf = new BinaryFormatter();
                    ret = (ProfileData)bf.Deserialize(file);

                    Debug.Log("LOADED SUCCESSFULY");
                }
            }
            catch
            {
                Debug.Log("FILE WASN'T FOUND");
            }

            return ret;
        }
    }
[System.Serializable]
public class ProfileData
{
    public string username;
    public string password;
    public string email;

    public ProfileData()
    {
        this.username = "";
        this.password = "";
        this.email = "";
    }

    public ProfileData(string u, string p, string e)
    {
        this.username = u;
        this.password = p;
        this.email = e;
    }
}
