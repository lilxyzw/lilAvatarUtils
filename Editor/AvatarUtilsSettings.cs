using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.avatarutils
{
    [FilePath("jp.lilxyzw/avatar-utils.asset", FilePathAttribute.Location.PreferencesFolder)]
    internal class AvatarUtilsSettings : ScriptableSingleton<AvatarUtilsSettings>
    {
        [Tooltip("The language setting for lilAvatarUtils. The language file exists in `jp.lilxyzw.avatar-utils/Editor/Localization`, and you can support other languages by creating a language file.")]
        public string language = CultureInfo.CurrentCulture.Name;

        internal void Save() => Save(true);
    }
}
