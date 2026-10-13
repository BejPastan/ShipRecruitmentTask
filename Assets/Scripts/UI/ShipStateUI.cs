using ShipRecruitmentTask.Ship;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace ShipRecruitmentTask.UI
{
    public class ShipStateUI : MonoBehaviour
    {
        [Inject]
        ShipManager _shipManager;

        [SerializeField]
        PanelRenderer uiDocument;

        Label rotLabel;
        Label flurLabel;



        private void Start()
        {
            if (_shipManager == null)
            {
                Debug.LogError($"{name} does not have ship manager attached");
            }
            _shipManager.WhenDestroying += OnSourceDestroy;
            uiDocument.RegisterUIReloadCallback(SetDataSource);
        }

        private void SetDataSource(PanelRenderer renderer, VisualElement root)
        {
            if (_shipManager != null)
            {
                rotLabel = root.Q<Label>("rot-label-val");
                rotLabel.SetBinding("text", new DataBinding()
                {
                    dataSource = _shipManager,
                    dataSourcePath = new Unity.Properties.PropertyPath(nameof(ShipManager.SailRot))//here
                });

                flurLabel = root.Q<Label>("flur-label-val");
                flurLabel.SetBinding("text", new DataBinding()
                {
                    dataSource = _shipManager,
                    dataSourcePath = new Unity.Properties.PropertyPath(nameof(ShipManager.SailFlur))
                });
            }
            uiDocument.UnregisterUIReloadCallback(SetDataSource);
        }

        public void OnSourceDestroy()
        {
            uiDocument.RegisterUIReloadCallback(UnregisterSource);
            _shipManager.WhenDestroying -= OnSourceDestroy;
        }

        private void UnregisterSource(PanelRenderer renderer, VisualElement root)
        {
            if (_shipManager != null)
            {
                rotLabel = root.Q<Label>("rot-label-val");
                rotLabel.SetBinding("text", new DataBinding()
                {
                    dataSource = null,
                });

                flurLabel = root.Q<Label>("flur-label-val");
                flurLabel.SetBinding("text", new DataBinding()
                {
                    dataSource = null
                });
            }
            uiDocument.UnregisterUIReloadCallback(UnregisterSource);
        }
    }
}