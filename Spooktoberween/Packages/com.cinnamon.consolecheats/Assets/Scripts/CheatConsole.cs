using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CheatSystem
{
    public class CheatConsole
    {
    #if USING_CHEAT_SYSTEM
        const int k_NumHistory = 15;
        string[] History = new string[k_NumHistory];
        int HistoryStart = 0;
        int HistoryLength = 0;
        int HistoryIdx = -1;

        int SuggestionIdx = -1;

        string CurrentCheatString;
        string CurrentDisplayString;
        public bool bIsEnabled = false;

        const int k_MaxSuggestions = 10;

        List<string> SuggestedCheats = new List<string>();

        void UpdateHistory()
        {
            HistoryIdx = -1;
            HistoryStart = (HistoryStart - 1 + k_NumHistory) % k_NumHistory;
            if (HistoryLength < k_NumHistory)
            {
                ++HistoryLength;
            }

            History[HistoryStart] = CurrentCheatString;
        }

        void Submit()
        {
            CheatSystem.ExecuteCheat(CurrentDisplayString);
            bIsEnabled = false;
            
            UpdateHistory();

            CurrentCheatString = "";
            CurrentDisplayString = "";
        }

        void ToggleEnabled()
        {
            bIsEnabled = !bIsEnabled;
            if (!bIsEnabled)
            {
                CurrentCheatString = "";
                CurrentDisplayString = "";
            }
        }

        public void DrawConsole()
        {
            if (Event.current.keyCode == KeyCode.BackQuote)
            {
                if (Event.current.type == EventType.KeyDown)
                {
                    ToggleEnabled();
                    Event.current.Use();
                }
            }

            if (bIsEnabled)
            {
                CheatSystem.GetCheatSuggestions(CurrentCheatString, k_MaxSuggestions, ref SuggestedCheats);

                if (Event.current.type == EventType.KeyDown && Event.current.keyCode != KeyCode.None)
                {
                    bool bUsedEvent = true;
                    int currentSuggestionIdx = -1;
                    bool bTabCompleted = false;

                    switch (Event.current.keyCode)
                    {
                        case KeyCode.Return:
                        {
                            Submit();
                            break; 
                        }
                        case KeyCode.UpArrow:
                        {
                            if (HistoryLength > 0)
                            {
                                if (HistoryIdx < 0)
                                {
                                    HistoryIdx = HistoryStart;
                                }
                                else
                                {
                                    int HistoryEnd = (HistoryStart + HistoryLength - 1 + k_NumHistory) % k_NumHistory;
                                    if (HistoryIdx != HistoryEnd)
                                    {
                                        HistoryIdx = (HistoryIdx + 1) % k_NumHistory;
                                    }
                                }

                                CurrentDisplayString = History[HistoryIdx];

                                MoveTextPosToEnd();
                            }
                            break;
                        }
                        case KeyCode.DownArrow:
                        {
                            if (HistoryIdx >= 0)
                            {
                                if (HistoryIdx != HistoryStart)
                                {
                                    HistoryIdx = (HistoryIdx - 1 + k_NumHistory) % k_NumHistory;
                                }

                                CurrentDisplayString = History[HistoryIdx];

                                MoveTextPosToEnd();
                            }

                            break;
                        }
                        case KeyCode.Tab:
                        {
                            if (SuggestedCheats.Count > 0)
                            {
                                bTabCompleted = true;

                                SuggestionIdx = (SuggestionIdx + 1) % SuggestedCheats.Count;

                                string[] Tokens = SuggestedCheats[SuggestionIdx].Split();
                                CurrentDisplayString = Tokens.Length > 0 ? Tokens[0] : SuggestedCheats[SuggestionIdx];

                                MoveTextPosToEnd();
                            }

                            break;
                        }
                        default:
                        {
                            bUsedEvent = false;
                            break;
                        }
                    }

                    if (bUsedEvent)
                    {
                        Event.current.Use();
                    }

                    if (!bTabCompleted)
                    {
                        SuggestionIdx = -1;
                    }
                }

                GUILayout.BeginVertical(GUILayout.Height(Screen.height));
                GUILayout.FlexibleSpace();

                GUILayout.BeginVertical(GUI.skin.box);
                if (SuggestedCheats.Count > 0)
                {
                    foreach (string SuggestedCheat in SuggestedCheats)
                    {
                        GUI.SetNextControlName(SuggestedCheat);
                        GUILayout.Label(SuggestedCheat);
                    }
                }

                GUIStyle consoleStyle = new GUIStyle(GUI.skin.textField);
                consoleStyle.fixedWidth = Screen.width - (consoleStyle.padding.left + consoleStyle.padding.right) - (GUI.skin.box.padding.left + GUI.skin.box.padding.right);

                const string consoleName = "Console";
                GUI.SetNextControlName(consoleName);
                if (Event.current.character == '`')
                {
                    GUILayout.TextField(CurrentDisplayString, consoleStyle);
                }
                else
                {
                    CurrentDisplayString = GUILayout.TextField(CurrentDisplayString, consoleStyle);
                }

                GUILayout.EndVertical();
                GUILayout.EndVertical();

                if (SuggestionIdx < 0)
                {
                    CurrentCheatString = CurrentDisplayString;
                }

                UpdateTextPos();
            }
        }

        void UpdateTextPos()
        {
            if (Event.current.type == EventType.Layout && bNeedsMoveLineEnd)
            {
                TextEditor textEditor =
                    (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                textEditor.MoveLineEnd();

                bNeedsMoveLineEnd = false;
            }
        }

        bool bNeedsMoveLineEnd = false;
        void MoveTextPosToEnd()
        {
            bNeedsMoveLineEnd = true;
        }
    #endif // USING_CHEAT_SYSTEM
    }
}