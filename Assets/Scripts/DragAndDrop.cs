using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.PlayerLoop;

public class DragAndDrop : MonoBehaviour
{
    [SerializeField] private UIItemSlot cursorSlot = null;
    private ItemSlot cursorItemSlot;

    [SerializeField] private GraphicRaycaster m_Raycaster = null;
    private PointerEventData m_PointerEventData;
    [SerializeField] private EventSystem m_EventSystem;


    private bool changedItem = false;
    private bool isFromCreativeMenu = false;
    private UIItemSlot swapClickedSlot = null;
    private UIItemSlot lastClickedSlot = null;
    
    private World world;
    
    private void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        
        cursorItemSlot = new ItemSlot(cursorSlot);
    }

    public void OnSetActive()
    {
        if(lastClickedSlot == null) return;

        if (isFromCreativeMenu)
        {
            cursorItemSlot.EmptySlot();
        }

        if (swapClickedSlot != null)
        {
            if(cursorItemSlot.stack != null)
                swapClickedSlot.itemSlot.InsertStack(cursorItemSlot.TakeAll());
            swapClickedSlot.UpdateSlot();
        }
        
        if (changedItem && !isFromCreativeMenu)
        {
            if(cursorItemSlot.stack != null)
                lastClickedSlot.itemSlot.InsertStack(cursorItemSlot.TakeAll());
            lastClickedSlot.UpdateSlot();
        }

        isFromCreativeMenu = false;
        changedItem = false;
    }

    private void Update()
    {
        if (!world.inUI) return;

        cursorSlot.transform.position = Input.mousePosition;
        
        if (Input.GetMouseButtonDown(0))
        {
            if (CheckForSlot() != null)
            {
                HandleSlotClick(CheckForSlot());
            } 
        }
        
        if (cursorSlot.HasItem && Input.GetMouseButtonDown(1) && !changedItem)
        {
            lastClickedSlot.itemSlot.InsertStack(cursorItemSlot.TakeAll());
            lastClickedSlot.UpdateSlot();
        }else if (cursorSlot.HasItem && Input.GetMouseButtonDown(1) && changedItem)
        {
            cursorItemSlot.EmptySlot();
        }
    }

    private void HandleSlotClick(UIItemSlot clickedSlot)
    {
        if(clickedSlot == null) return;
        
        if(!cursorSlot.HasItem && !clickedSlot.HasItem) return;

        lastClickedSlot = clickedSlot;

        if (clickedSlot.itemSlot.isCreative)
        {
            isFromCreativeMenu = true;
            cursorItemSlot.EmptySlot();
            cursorItemSlot.InsertStack(clickedSlot.itemSlot.stack);
        }
        
        if (!cursorSlot.HasItem && clickedSlot.HasItem)
        {
            changedItem = true;
            cursorItemSlot.InsertStack(clickedSlot.itemSlot.TakeAll());
            cursorSlot.UpdateSlot();
            swapClickedSlot = clickedSlot;
            return;
        }

        if (cursorSlot.HasItem && !clickedSlot.HasItem)
        {
            changedItem = true;
            clickedSlot.itemSlot.InsertStack(cursorItemSlot.TakeAll());
            clickedSlot.UpdateSlot();
            return;
        }
        
        if (cursorSlot.HasItem && clickedSlot.HasItem)
        {
            changedItem = true;
            if (cursorSlot.itemSlot.stack.id != clickedSlot.itemSlot.stack.id)
            {
                ItemStack oldCursorSlot = cursorSlot.itemSlot.TakeAll();
                ItemStack oldSlot = clickedSlot.itemSlot.TakeAll();
                
                
                
                cursorSlot.itemSlot.InsertStack(oldSlot);
                clickedSlot.itemSlot.InsertStack(oldCursorSlot);
            }
            // TODO: Handle same blocks in slots
        }
    }

    private UIItemSlot CheckForSlot()
    {
        m_PointerEventData = new PointerEventData(m_EventSystem);
        m_PointerEventData.position = Input.mousePosition;
        
        List<RaycastResult> results = new List<RaycastResult>();
        m_Raycaster.Raycast(m_PointerEventData,results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.CompareTag("UIItemSlot"))
            {
                return result.gameObject.GetComponent<UIItemSlot>();
            }
        }

        return null;
    }
}
