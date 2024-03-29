using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.localizator.dialog
{
    using fwp.localizator;

    /// <summary>
    /// LineData generic is to have control over content in dialog lines
    /// </summary>
    public class DialogManager<LineData> where LineData : LocaDialogLineData
    {
        static public DialogManager<LineData> instance;

        static public string folderDialogs = "dialogs/";
        static public string resourcesDialogs = "Resources/" + folderDialogs;

        static public string assetDialogs = System.IO.Path.Combine(
            "Assets/", resourcesDialogs);
        static public string sysDialogs = System.IO.Path.Combine(
            Application.dataPath, resourcesDialogs);

        IsoLanguages iso => LocalizationManager.instance.getSavedIsoLanguage();

        // for one iso language
        public string[] dialogsUids;

        public LocaDialogData<LineData>[] dialogs;

        public DialogManager()
        {
            instance = this;
            refresh();
        }

        public LocaDialogData<LineData> getDialogInstance(string uid)
        {
            foreach (var d in dialogs)
            {
                if (d == null)
                    continue;

                if (d.name == uid)
                    return d;
            }
            return null;
        }

        public bool hasDialogInstance(string uid)
        {
            foreach (var d in dialogs)
            {
                if (d.name == uid)
                    return true;
            }
            return false;
        }

        public void refresh()
        {
            var mgr = LocalizationManager.instance;
            if (mgr == null)
            {
                Debug.LogError("no loca manager ?");
                return;
            }

            // get french (default)
            var file = mgr.getFileByLang(iso);
            if (file == null)
            {
                Debug.LogWarning("no fr file ?");
                return;
            }

            // get scriptables
            dialogs = getDialogs();

            List<string> tmp = new List<string>();

            // all lines in lang file
            var lines = file.getLines();
            foreach (var l in lines)
            {
                // split UID=VAL
                var split = l.Split("=");
                var uid = split[0];

                // split UID-{NUM}
                uid = uid.Substring(0, uid.LastIndexOf("-"));

                if (!tmp.Contains(uid))
                    tmp.Add(uid);
            }
            
            dialogsUids = tmp.ToArray();

            Debug.Log($"{iso} -> solved x{dialogsUids.Length} dialog uids");
        }

        protected LocaDialogData<LineData>[] getDialogs()
        {
#if UNITY_EDITOR
            return LocalizatorUtils.getScriptableObjectsInEditor<LocaDialogData<LineData>>();
#else
            return null;
#endif
        }
    }
}
