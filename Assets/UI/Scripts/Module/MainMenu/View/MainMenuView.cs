using BH.UI.Scripts.Base;
using BH.UI.Scripts.Module.MainMenu.ViewModel;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace BH.UI.Scripts.Module.MainMenu.View
{
    public class MainMenuView : ViewBase
    {
        [Header("主菜单UI元素")] [SerializeField] private Image background;
        [SerializeField] private Button battleBtn;
        [SerializeField] private Button battleSettingBtn;
        [SerializeField] private Button settingsBtn;
        [SerializeField] private Button exitBtn;

        [Inject] private MainMenuViewModel _viewModel;

        protected override void BindUI()
        {
            // 绑定按钮点击事件到ViewModel
            battleBtn.OnClickAsObservable()
                .Subscribe(_ => _viewModel.OnBattleClick.OnNext(Unit.Default))
                .AddTo(ViewDisposables);

            battleSettingBtn.OnClickAsObservable()
                .Subscribe(_ => _viewModel.OnBattleSettingClick.OnNext(Unit.Default))
                .AddTo(ViewDisposables);

            settingsBtn.OnClickAsObservable()
                .Subscribe(_ => _viewModel.OnSettingsClick.OnNext(Unit.Default))
                .AddTo(ViewDisposables);

            exitBtn.OnClickAsObservable()
                .Subscribe(_ => _viewModel.OnExitClick.OnNext(Unit.Default))
                .AddTo(ViewDisposables);

            // 绑定ViewModel状态到UI
            _viewModel.IsBattleBtnEnabled
                .Subscribe(isEnabled => battleBtn.interactable = isEnabled)
                .AddTo(ViewDisposables);
        }

        /// <summary>
        /// 重写显示逻辑
        /// </summary>
        public override void Show()
        {
            base.Show();
            MainMenuShowAnimation();
        }

        /// <summary>
        /// 主菜单渐入动画
        /// </summary>
        void MainMenuShowAnimation()
        {
            var allButtons = new[] { battleBtn, battleSettingBtn, settingsBtn, exitBtn };

            // 初始化状态：全隐藏
            background.color = new Color(1, 1, 1, 0);
            foreach (var btn in allButtons)
            {
                var rt = btn.GetComponent<RectTransform>();
                var graphic = btn.targetGraphic;
                var text = btn.GetComponentInChildren<TMP_Text>();

                rt.anchoredPosition += new Vector2(-300, 0); // 左侧外
                graphic.color = new Color(1, 1, 1, 0);
                if (text) text.color = new Color(0.1960784f, 0.1960784f, 0.1960784f, 0);
            }

            // 播放序列动画
            var seq = DOTween.Sequence();

            // 背景淡入
            seq.Append(background.DOFade(1, 0.6f).SetEase(Ease.OutQuad));

            // 按钮依次滑入 + 淡入
            foreach (var btn in allButtons)
            {
                var rt = btn.GetComponent<RectTransform>();
                var graphic = btn.targetGraphic;
                var text = btn.GetComponentInChildren<TMP_Text>();

                seq.AppendInterval(0.08f); // 间隔
                seq.Append(rt.DOAnchorPosX(rt.anchoredPosition.x + 300, 0.5f).SetEase(Ease.OutBack));
                seq.Join(graphic.DOFade(1, 0.4f));
                if (text) seq.Join(text.DOFade(1, 0.4f));
            }

            seq.OnComplete(() =>
            {
                foreach (var btn in allButtons) btn.interactable = true;
            });
        }
    }
}