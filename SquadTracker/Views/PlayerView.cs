﻿using System;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using Torlando.SquadTracker.Presenters;

namespace Torlando.SquadTracker.Views
{
    class PlayerView : View<PlayerPresenter>
    {
        #region UI
        private DetailsButton _detailsButton;
        private Dropdown _dropdown1;
        private Dropdown _dropdown2;
        private Image _roleIcon1 = new Image { Size = new Point(27, 27) };
        private Image _roleIcon2 = new Image { Size = new Point(27, 27) };
        private Panel _activePlayerPanel;
        private Panel _formerPlayerPanel;
        private Func<uint, uint, AsyncTexture2D> _iconGetter;
        private const string _placeholderRoleName = "Select a role...";
        #endregion

        protected override void Build(Container buildPanel)
        {
            _detailsButton = new DetailsButton
            {
                Parent = _activePlayerPanel,
                //Text = $"{_player.CharacterName} ({_player.AccountName})",
                IconSize = DetailsIconSize.Small,
                ShowVignette = true,
                HighlightType = DetailsHighlightType.LightHighlight,
                ShowToggleButton = true,
                //Icon = _iconGetter(_player.Profession, _player.CurrentSpecialization),
                Size = new Point(354, 90)
            };



            //base.Build(buildPanel); // do we need base.Build() ?
        }

        public void SetPlayerText(string playerText)
        {
            _detailsButton.Text = playerText;
        }

        public void SetPlayerIcon(AsyncTexture2D icon)
        {
            _detailsButton.Icon = icon;
        }
    }
}
