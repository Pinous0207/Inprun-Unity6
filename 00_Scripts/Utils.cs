using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.U2D;

public class Utils : MonoBehaviour
{
    public static SpriteAtlas atlas = Resources.Load<SpriteAtlas>("Atlas");
    public static Sprite GetAtlas(string name)
    {
        Debug.Log(name);
        return atlas.GetSprite(name);
    }

    public static string Localization_Text(Localize localize, string key)
    {
        Locale currentLanguage = LocalizationSettings.SelectedLocale;
        string localizedString = LocalizationSettings.StringDatabase.GetLocalizedString(localize.ToString(), key, currentLanguage);
        return localizedString;
    }
}
