using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MusicGameInstManager : MonoBehaviour
{
	[SerializeField] private List<GameObject> pages = new List<GameObject>();
	[SerializeField] private int startingPageIndex = 0;
	[SerializeField] private bool wrapAround = false;

	[SerializeField] private Button nextButton;
	[SerializeField] private Button prevButton;

	[SerializeField] private bool enableKeyboardNavigation = true;
	[SerializeField] private KeyCode nextKey = KeyCode.RightArrow;
	[SerializeField] private KeyCode prevKey = KeyCode.LeftArrow;

	[SerializeField] private bool collectPagesFromChildren = true;
	[SerializeField] private Transform pagesRoot;

	private int currentIndex = -1;

	private void Awake()
	{
		if (collectPagesFromChildren)
		{
			CollectPages();
		}
	}

	private void OnEnable()
	{
		if (pages.Count == 0 && collectPagesFromChildren)
		{
			CollectPages();
		}

		BindButtonListeners();

		int safeStart = Mathf.Clamp(startingPageIndex, 0, Mathf.Max(pages.Count - 1, 0));
		ShowPage(safeStart);
	}

	private void OnDisable()
	{
		UnbindButtonListeners();
	}

	private void Update()
	{
		if (!enableKeyboardNavigation) return;
		if (Input.GetKeyDown(nextKey))
		{
			OnNextButton();
		}
		else if (Input.GetKeyDown(prevKey))
		{
			OnPrevButton();
		}
	}

	private void CollectPages()
	{
		Transform root = pagesRoot != null ? pagesRoot : transform;
		pages.Clear();
		for (int i = 0; i < root.childCount; i++)
		{
			GameObject child = root.GetChild(i).gameObject;
			pages.Add(child);
		}
	}

	private void BindButtonListeners()
	{
		if (nextButton != null)
		{
			nextButton.onClick.AddListener(OnNextButton);
		}
		if (prevButton != null)
		{
			prevButton.onClick.AddListener(OnPrevButton);
		}
	}

	private void UnbindButtonListeners()
	{
		if (nextButton != null)
		{
			nextButton.onClick.RemoveListener(OnNextButton);
		}
		if (prevButton != null)
		{
			prevButton.onClick.RemoveListener(OnPrevButton);
		}
	}

	public void OnNextButton()
	{
		NextPage();
	}

	public void OnPrevButton()
	{
		PreviousPage();
	}

	public void NextPage()
	{
		if (pages.Count == 0) return;
		int next = currentIndex + 1;
		if (next >= pages.Count)
		{
			next = wrapAround ? 0 : pages.Count - 1;
		}
		ShowPage(next);
	}

	public void PreviousPage()
	{
		if (pages.Count == 0) return;
		int prev = currentIndex - 1;
		if (prev < 0)
		{
			prev = wrapAround ? pages.Count - 1 : 0;
		}
		ShowPage(prev);
	}

	public void SelectPage(int index)
	{
		ShowPage(index);
	}

	public void ShowPage(int index)
	{
		if (pages.Count == 0) return;
		index = Mathf.Clamp(index, 0, pages.Count - 1);

		for (int i = 0; i < pages.Count; i++)
		{
			if (pages[i] != null)
			{
				pages[i].SetActive(i == index);
			}
		}

		currentIndex = index;
		UpdateButtonState();
	}

	private void UpdateButtonState()
	{
		if (nextButton != null)
		{
			nextButton.interactable = wrapAround || currentIndex < pages.Count - 1;
		}
		if (prevButton != null)
		{
			prevButton.interactable = wrapAround || currentIndex > 0;
		}
	}

	public int GetCurrentPageIndex()
	{
		return currentIndex;
	}

	public int GetPageCount()
	{
		return pages.Count;
	}
}