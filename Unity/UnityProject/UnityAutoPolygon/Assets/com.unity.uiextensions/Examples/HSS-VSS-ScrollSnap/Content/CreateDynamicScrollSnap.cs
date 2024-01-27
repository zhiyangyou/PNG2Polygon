using UnityEngine;
using UnityEngine.UI.Extensions;

public class CreateDynamicScrollSnap : MonoBehaviour
{
    [SerializeField]
    private GameObject ScrollSnapPrefab;

    private HorizontalScrollSnap hss;

    [SerializeField]
    private GameObject ScrollSnapContent;

    [SerializeField]
    private int startingPage = 0;

    private bool isInitialized = false;

    // Start is called before the first frame update
    void Start()
    {
        hss = Instantiate(ScrollSnapPrefab, this.transform).GetComponent<HorizontalScrollSnap>();
        hss.ChangePage(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isInitialized && hss != null && Input.GetKeyDown(KeyCode.K))
        {
            AddHSSChildren();
            isInitialized = true;
        }
    }

    private void AddHSSChildren()
    {
        var contentGO = hss.transform.Find("Content");
        if (contentGO != null)
        {
            for (int i = 0; i < 10; i++)
            {
                GameObject item = Instantiate(ScrollSnapContent);
                SetHSSItemTest(item, $"Page {i}");
                hss.AddChild(item);
            }
            hss.StartingScreen = startingPage;
            hss.UpdateLayout(true);
        }
        else
        {
            Debug.LogError("Content not found");
        }
    }

    private void SetHSSItemTest(GameObject prefab, string value)
    {
        prefab.gameObject.name = value;
        prefab.GetComponentInChildren<UnityEngine.UI.Text>().text = value;
    }
}
