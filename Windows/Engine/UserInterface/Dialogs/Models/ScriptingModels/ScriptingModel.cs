﻿using Org.InCommon.InCert.Engine.AdvancedMenu;
using Org.InCommon.InCert.Engine.Engines;
using Org.InCommon.InCert.Engine.Help;
using Org.InCommon.InCert.Engine.Results.ControlResults;
using Org.InCommon.InCert.Engine.Settings;
using Org.InCommon.InCert.Engine.UserInterface.Dialogs.Models.DialogModels;

namespace Org.InCommon.InCert.Engine.UserInterface.Dialogs.Models.ScriptingModels
{
    public class ScriptingModel : IScriptingModel
    {
        private readonly ISettingsManager _settingsManager;
        private readonly IHelpManager _helpManager;
        private readonly IAdvancedMenuManager _advancedMenuManager;
        private readonly IHasEngineFields _engine;
        private readonly AbstractDialogModel _dialogModel;


        public ScriptingModel(IHasEngineFields engine, AbstractDialogModel dialogModel)
        {
            _settingsManager = engine.SettingsManager;
            _helpManager = engine.HelpManager;
            _advancedMenuManager = engine.AdvancedMenuManager;
            _engine = engine;
            _dialogModel = dialogModel;
        }
        
        public bool InCertPresent()
        {
            return true;
        }

        public string GetValue(string key)
        {
            return _settingsManager.GetTemporarySettingString(key);
        }

        public void SetValue(string key, string value)
        {
            _settingsManager.SetTemporarySettingString(key, value);
        }

        public void ReturnNext()
        {
            _dialogModel.Result = new NextResult();
        }

        public void ReturnBack()
        {
            _dialogModel.Result = new BackResult();
        }

        public void ShowAdvancedMenu(string group="")
        {
            throw new System.NotImplementedException();
        }

        public void ShowHelpTopic(string value)
        {
            _helpManager.ShowHelpTopic(value, _dialogModel);
        }
    }
}