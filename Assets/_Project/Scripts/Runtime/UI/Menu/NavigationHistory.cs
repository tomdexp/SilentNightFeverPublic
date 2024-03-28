using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Scripts.Runtime.UI
{
    public class NavigationHistory : MonoBehaviour
    {
        // Navigation history
        [SerializeField] public List<HistoryInstruction2> Instructions = new List<HistoryInstruction2>();

        public void AddInstructionToHistory(Menu menu)
        {
            HistoryInstruction2 instruction;
            instruction.selectedUI = EventSystem.current.currentSelectedGameObject;
            instruction.selectedMenu = menu;
            Instructions.Add(instruction);
        }

        public void AddInstructionToHistory()
        {
            HistoryInstruction2 instruction;
            instruction.selectedUI = EventSystem.current.currentSelectedGameObject;
            instruction.selectedMenu = null;
            Instructions.Add(instruction);
        }

        public void RevertLastInstruction()
        {
            if (Instructions.Count == 0) return;

            HistoryInstruction2 lastInstruction = Instructions.Last();
            if (lastInstruction.selectedMenu)
                lastInstruction.selectedMenu.OpenParentMenu(true);
            else
                EventSystem.current.SetSelectedGameObject(lastInstruction.selectedUI.gameObject);

            Instructions.Remove(lastInstruction);
        }
    }

    public struct HistoryInstruction2
    {
        public GameObject selectedUI;
        public Menu selectedMenu;
    }
}