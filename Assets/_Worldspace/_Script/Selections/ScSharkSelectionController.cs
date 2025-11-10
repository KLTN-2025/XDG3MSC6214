using _Workspace._Scripts.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Workspace._Scripts.Selections
{
    public class ScSharkSelectionController : MonoBehaviour
    {
        [Header("Data")]
    [SerializeField] private ScSharkCatalog catalog;
    [SerializeField] private int defaultId = 1001;

    [Header("Preview")]
    [SerializeField] private Transform previewRoot;
    [SerializeField] private float autoRotateY = 25f;
    [SerializeField] private string menuIdleState = "Menu_Idle";

    [Header("UI (optional)")]
    [SerializeField] private Image iconImage;
#if TMP_PRESENT
    [SerializeField] private TMP_Text titleText;
#endif

    [Header("Flow")]
    [SerializeField] private string gameSceneName = "Game"; 

    GameObject _currentPreview;
    private int _currentIndex;

    private void Start()
    {

        int savedId = ScSharkSelectionService.Get(defaultId);
        _currentIndex = catalog.IndexOfId(savedId);
        if (_currentIndex < 0) _currentIndex = 0;
        Show(_currentIndex);
    }

    void Update()
    {
        if (_currentPreview)
            _currentPreview.transform.Rotate(Vector3.up, autoRotateY * Time.deltaTime, Space.World);
    }
    
    public void Next()   => Show((_currentIndex + 1) % catalog.Count);
    public void Prev()   => Show((_currentIndex - 1 + catalog.Count) % catalog.Count);
    public void Randomize()
    {
        if (catalog.Count <= 0) return;
        int r;
        do { r = Random.Range(0, catalog.Count); } while (r == _currentIndex && catalog.Count > 1);
        Show(r);
    }

    public void ConfirmSelection()
    {
        var e = catalog.Get(_currentIndex);
        if (e != null) ScSharkSelectionService.Set(e.id);
    }

    public void ConfirmAndStartGame()
    {
        ConfirmSelection();
        if (!string.IsNullOrEmpty(gameSceneName))
            SceneManager.LoadScene(gameSceneName);
    }

    private void Show(int index)
    {
        var e = catalog.Get(index);
        if (e == null || e.previewPrefab == null) return;

        if (_currentPreview) Destroy(_currentPreview);
        _currentPreview = Instantiate(e.previewPrefab, previewRoot);
        _currentPreview.transform.localPosition = Vector3.zero;
        _currentPreview.transform.localRotation = Quaternion.identity;
        _currentPreview.transform.localScale    = Vector3.one;

        var anim = _currentPreview.GetComponentInChildren<Animator>();
        if (anim && !string.IsNullOrEmpty(menuIdleState))
            anim.Play(menuIdleState, 0, 0f);
        
        if (iconImage) iconImage.sprite = e.icon;
#if TMP_PRESENT
        if (titleText) titleText.text = e.displayName;
#endif
        _currentIndex = index;
    }
    }
}
