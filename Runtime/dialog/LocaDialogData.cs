﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace fwp.localizator.dialog
{
    /// <summary>
    /// 
    /// dialog UID in spreadsheet must match name of scriptable
    /// 
    /// </summary>
    //[CreateAssetMenu(menuName = LocalizationManager._asset_menu_path + "create dialog data",fileName = "DialogData_", order = LocalizationManager._asset_menu_order)]
    public class LocaDialogData<LineData> : ScriptableObject where LineData : LocaDialogLineData
    {
        public const string dialog_line_number_separator = "-";

        public string locaId => name;

        [SerializeField]
        public LineData[] lines;

        public LineData getNextLine(LineData line)
        {
            Debug.Assert(line != null);

            Debug.Log(GetType() + " :: " + name + " :: getNextLine() :: searching next line, in x" + lines.Length + " possible lines");
            Debug.Log("  L from line " + line.uid + " (" + line.getSolvedLineByUID(false) + ")");

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i] == line)
                {
                    Debug.Log("  L found line at index " + i + ", returning next LineData");

                    // last, no next
                    if (i >= lines.Length - 1) return null;

                    return lines[i + 1];
                }
            }

            Debug.LogWarning("  L /! line wasn't in dialog ?");
            return null;
        }

#if UNITY_EDITOR

        virtual public void solveContent()
        {
            editorSolveLines();
        }

        protected string getCellValue(string lineUid, int cellIndex)
        {
            string line = fetchSheetLine(lineUid);

            //Debug.Log(lineUid + " = " + line);

            if(!string.IsNullOrEmpty(line))
            {
                //Debug.Log(line);
                return line.Split(",")[cellIndex];
            }

            return string.Empty;
        }

        string fetchSheetLine(string uid)
        {
            //Debug.Log(uid);
            var sheets = LocalizatorUtils.getSheetsData();
            foreach (var s in sheets)
            {
                foreach (var t in s.sheetTabsIds)
                {
                    var txt = t.getLine(uid);
                    if(!string.IsNullOrEmpty(txt))
                    {
                        return txt;
                    }
                }
            }
            return string.Empty;
        }

        public void editorSolveLines()
        {
            if (locaId == null)
            {
                Debug.LogWarning(name + " has no loca id", this);
                return;
            }

            if (locaId.Length <= 0)
            {
                Debug.LogWarning(name + " loca id is empty", this);
                return;
            }

            Debug.Log("[" + LocalizationManager.instance.getSavedIsoLanguage() + "] solve lines @ " + locaId);

            List<LineData> tmp = new List<LineData>();

            int safe = 50;
            int index = 1;
            string ct;

            do
            {
                string fullId = locaId + dialog_line_number_separator + ((index < 10) ? "0" + index : index.ToString());
                ct = LocalizationManager.instance.getContent(fullId);

                if (ct.IndexOf("['") > -1) ct = string.Empty;

                if (ct.Length > 0)
                {
                    LineData line = System.Activator.CreateInstance<LineData>();
                    line.uid = fullId;

                    Debug.Log("     ADD fid ? " + fullId + " => " + ct);

                    tmp.Add(line);
                }

                index++;
                safe--;

            } while (safe > 0 && ct.Length > 0);


            //string mergeLog = string.Empty;

            if (lines == null || lines.Length <= 0)
            {
                if (tmp.Count > 0)
                {
                    lines = tmp.ToArray();
                }
            }
            else
            {
                List<LineData> merged = new List<LineData>();

                for (int i = 0; i < tmp.Count; i++)
                {
                    if (i < lines.Length)
                    {
                        if (lines[i].getSolvedLineByFUID() == tmp[i].getSolvedLineByFUID())
                            merged.Add(lines[i]);
                        else
                        {
                            merged.Add(tmp[i]);
                        }
                    }
                    else
                    {
                        merged.Add(tmp[i]);
                    }
                }

                lines = merged.ToArray();

                //mergeLog = "[Merged]";
            }

            cmUpdateCached();
        }

        public void cmUpdateCached()
        {
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i].debugUpdatePreview(false);
            }
        }

        static public LocaDialogData<LineData>[] getScriptables()
        {
            return LocalizationStatics.getScriptableObjectsInEditor<LocaDialogData<LineData>>();
        }

        [MenuItem(LocalizationManager._menu_item_path + "dialogs/solve all dialog lines")]
        static protected void solveLines()
        {
            LocaDialogData<LineData>[] all = getScriptables();

            float progress = 0f;
            for (int i = 0; i < all.Length; i++)
            {
                progress = (float)(i + 1f) / Mathf.Max(1f, (float)all.Length);

                if (EditorUtility.DisplayCancelableProgressBar("Solving all dialog lines", "Solving " + all[i].name + " (" + (i + 1) + "/" + all.Length + ")", progress))
                {
                    EditorUtility.ClearProgressBar();
                    return;
                }

                all[i].cmUpdateCached();
                EditorUtility.SetDirty(all[i]);
            }
            AssetDatabase.SaveAssets();

            EditorUtility.ClearProgressBar();
        }

        [MenuItem(LocalizationManager._menu_item_path + "dialogs/solve all dialog lines NO DIFF")]
        static protected void solveLinesNoDiff()
        {
            LocaDialogData<LineData>[] all = getScriptables();

            float progress = 0f;
            for (int i = 0; i < all.Length; i++)
            {
                progress = (float)(i + 1f) / Mathf.Max(1f, (float)all.Length);
                if (EditorUtility.DisplayCancelableProgressBar("Solving all dialog lines", "Solving " + all[i].name + " (" + (i + 1) + "/" + all.Length + ")", progress))
                {
                    EditorUtility.ClearProgressBar();
                    return;
                }

                all[i].editorSolveLines();

                EditorUtility.SetDirty(all[i]);
            }
            AssetDatabase.SaveAssets();

            EditorUtility.ClearProgressBar();
        }

#endif

    }

}
