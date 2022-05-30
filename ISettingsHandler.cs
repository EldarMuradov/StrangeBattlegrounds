public interface ISettingsHandler 
{
    void Exit(int num);
    void Settings();
}
public interface IPauseHandler
{
    void Pause();
    void Continue();
}