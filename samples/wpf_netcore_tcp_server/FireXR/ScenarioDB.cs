using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FireXR.Protobuf;
using FireXR.Util;

namespace FireXR
{

    public class ScenarioDB
    {
        public readonly Dictionary<uint, Transform> Transforms = new Dictionary<uint, Transform>();
        public readonly Dictionary<uint, InteractionPoint> InteractionPoints = new Dictionary<uint, InteractionPoint>();
        public readonly Dictionary<uint, CutScene> CutScenes = new Dictionary<uint, CutScene>();
        public readonly Dictionary<uint, ObjectInfo> ObjectInfos = new Dictionary<uint, ObjectInfo>();
        public readonly Dictionary<uint, Sound> Sounds = new Dictionary<uint, Sound>();
        public readonly Dictionary<uint, FDSFile> FDSFiles = new Dictionary<uint, FDSFile>();
        public readonly Dictionary<uint, FDS> FDSs = new Dictionary<uint, FDS>();
        public readonly Dictionary<uint, XREvent> XREvents = new Dictionary<uint, XREvent>();
        public readonly Dictionary<uint, EvaluationAction> EvaluationActions = new Dictionary<uint, EvaluationAction>();
        public readonly Dictionary<uint, Evaluation> Evaluations = new Dictionary<uint, Evaluation>();
        public readonly Dictionary<uint, SeparatedScenario> SeparatedScenarios = new Dictionary<uint, SeparatedScenario>();
        public readonly Dictionary<uint, CombinedScenario> CombinedScenarios = new Dictionary<uint, CombinedScenario>();

        public readonly Dictionary<uint, CombinedScenario> FullScenario = new Dictionary<uint, CombinedScenario>();

        private NetworkManager _networkManager;

        private string _apiRootPath = "/";
        private string _apiUnitPath = "unit/";
        private string _apiIntegratedPath = "integrated/";
        private string _localDataPath = "Media/";
        private string _transformFileName = $"{nameof(Transform).ToLower()}.json";
        private string _interactionPointFileName = "interactionpoint.json";
        private string _cutsceneFileName = "cutscene.json";
        private string _objectInfoFileName = "objectinfo.json";
        private string _soundFileName = "sound.json";
        private string _fdsFileFileName = "fdsfile.json";
        private string _fdsFileName = "fds.json";
        private string _xrEventFileName = "xrevent.json";
        private string _evaluationActionFileName = "evaluationaction.json";
        private string _evaluationFileName = "evaluation.json";
        private string _separatedScenarioFileName = "separatedscenario.json";
        private string _combinedScenarioFileName = "combinedscenario.json";


        public ScenarioDB(NetworkManager networkManager)
        {
            if (networkManager == null)
            {
                throw new ArgumentNullException("NetworkManager must be included");
            }
            _networkManager = networkManager;
        }

        public void UploadAll()
        {
            LoadAll();
            PushAll();
        }

        public void DownloadAll()
        {
            PullAll();
            SaveAll();
        }

        public async void PushAll()
        {
            await PushTransforms();
            await PushInteractionPoints();
            await PushCutScenes();
            await PushObjectInfos();
            await PushSounds();
            await PushFDSFiles();
            await PushFDSs();
            await PushXREvents();
            await PushEvaluationActions();
            await PushEvaluations();
            await PushSeparatedScenarios();
            await PushCombinedScenarios();
        }

        public void PullAll()
        {
            PullTransforms();
            PullInteractionPoints();
            PullCutScenes();
            PullObjectInfos();
            PullSounds();
            PullFDSFiles();
            PullFDSs();
            PullXREvents();
            PullEvaluationActions();
            PullEvaluations();
            PullSeparatedScenarios();
            PullCombinedScenarios();
        }

        public void LoadAll()
        {
            LoadTransforms();
            LoadInteractionPoints();
            LoadCutScenes();
            LoadObjectInfos();
            LoadSounds();
            LoadFDSFiles();
            LoadFDSs();
            LoadXREvents();
            LoadEvaluationActions();
            LoadEvaluations();
            LoadSeparatedScenarios();
            LoadCombinedScenarios();
        }

        public void SaveAll()
        {
            SaveTransforms();
            SaveInteractionPoints();
            SaveCutScenes();
            SaveObjectInfos();
            SaveSounds();
            SaveFDSFiles();
            SaveFDSs();
            SaveXREvents();
            SaveEvaluationActions();
            SaveEvaluations();
            SaveSeparatedScenarios();
            SaveCombinedScenarios();
        }

        #region Load Data
        public void LoadTransforms()
        {
            JsonUtil.ReadFileOrDefault<List<Transform>>(_localDataPath + _transformFileName)
                .ForEach(item => Transforms[item.ID] = item);
        }

        public void LoadInteractionPoints()
        {
            JsonUtil.ReadFileOrDefault<List<InteractionPoint>>(_localDataPath + _interactionPointFileName)
                .ForEach(item => InteractionPoints[item.ID] = item);
        }

        public void LoadCutScenes()
        {
            JsonUtil.ReadFileOrDefault<List<CutScene>>(_localDataPath + _cutsceneFileName)
                .ForEach(item => CutScenes[item.ID] = item);
        }

        public void LoadObjectInfos()
        {
            JsonUtil.ReadFileOrDefault<List<ObjectInfo>>(_localDataPath + _objectInfoFileName)
                .ForEach(item => ObjectInfos[item.ID] = item);
        }

        public void LoadSounds()
        {
            JsonUtil.ReadFileOrDefault<List<Sound>>(_localDataPath + _soundFileName)
                .ForEach(item => Sounds[item.ID] = item);
        }

        public void LoadFDSFiles()
        {
            JsonUtil.ReadFileOrDefault<List<FDSFile>>(_localDataPath + _fdsFileFileName)
                .ForEach(item => FDSFiles[item.ID] = item);
        }

        public void LoadFDSs()
        {
            JsonUtil.ReadFileOrDefault<List<FDS>>(_localDataPath + _fdsFileName)
                .ForEach(item => FDSs[item.ID] = item);
        }

        public void LoadXREvents()
        {
            JsonUtil.ReadFileOrDefault<List<XREvent>>(_localDataPath + _xrEventFileName)
                .ForEach(item => XREvents[item.ID] = item);
        }

        public void LoadEvaluationActions()
        {
            JsonUtil.ReadFileOrDefault<List<EvaluationAction>>(_localDataPath + _evaluationActionFileName)
                .ForEach(item => EvaluationActions[item.ID] = item);
        }

        public void LoadEvaluations()
        {
            JsonUtil.ReadFileOrDefault<List<Evaluation>>(_localDataPath + _evaluationFileName)
                .ForEach(item => Evaluations[item.ID] = item);
        }

        public void LoadSeparatedScenarios()
        {
            JsonUtil.ReadFileOrDefault<List<SeparatedScenario>>(_localDataPath + _separatedScenarioFileName)
                .ForEach(item => SeparatedScenarios[item.ID] = item);
        }

        public void LoadCombinedScenarios()
        {
            JsonUtil.ReadFileOrDefault<List<CombinedScenario>>(_localDataPath + _combinedScenarioFileName)
                .ForEach(item => CombinedScenarios[item.ID] = item);
        }
        #endregion

        #region Save Data
        public void SaveTransforms()
        {
            JsonUtil.WriteFile(_localDataPath + _transformFileName, Transforms);
        }

        public void SaveInteractionPoints()
        {
            JsonUtil.WriteFile(_localDataPath + _interactionPointFileName, InteractionPoints);
        }

        public void SaveCutScenes()
        {
            JsonUtil.WriteFile(_localDataPath + _cutsceneFileName, CutScenes);
        }

        public void SaveObjectInfos()
        {
            JsonUtil.WriteFile(_localDataPath + _objectInfoFileName, ObjectInfos);
        }

        public void SaveSounds()
        {
            JsonUtil.WriteFile(_localDataPath + _soundFileName, Sounds);
        }

        public void SaveFDSFiles()
        {
            JsonUtil.WriteFile(_localDataPath + _fdsFileFileName, FDSFiles);
        }

        public void SaveFDSs()
        {
            JsonUtil.WriteFile(_localDataPath + _fdsFileName, FDSs);
        }

        public void SaveXREvents()
        {
            JsonUtil.WriteFile(_localDataPath + _xrEventFileName, XREvents);
        }

        public void SaveEvaluationActions()
        {
            JsonUtil.WriteFile(_localDataPath + _evaluationActionFileName, EvaluationActions);
        }

        public void SaveEvaluations()
        {
            JsonUtil.WriteFile(_localDataPath + _evaluationFileName, Evaluations);
        }

        public void SaveSeparatedScenarios()
        {
            JsonUtil.WriteFile(_localDataPath + _separatedScenarioFileName, SeparatedScenarios);
        }

        public void SaveCombinedScenarios()
        {
            JsonUtil.WriteFile(_localDataPath + _combinedScenarioFileName, CombinedScenarios);
        }
        #endregion 

        #region Pull Data
        protected List<T> PullData<T>(string url) where T : new()
        {
            List<T> datas = null;
            try
            {
                var task = _networkManager.GetRequestAPIAsync(url);
                task.Wait();
                var response = task.Result;
                datas = JsonConvert.DeserializeObject<List<T>>(response,
                    new JsonSerializerSettings
                    {
                        DefaultValueHandling = DefaultValueHandling.Populate,
                        NullValueHandling = NullValueHandling.Ignore
                    });
            }
            catch (Exception e)
            {
            }

            if (datas == null)
            {
                datas = new List<T>();
            }

            return datas;
        }

        public void PullTransforms()
        {
            PullData<Transform>($"/unit/transform/")
                .ForEach(item => Transforms[item.ID] = item);
        }

        public void PullInteractionPoints()
        {
            PullData<InteractionPoint>($"/unit/interactionpoint/")
                .ForEach(item => InteractionPoints[item.ID] = item);
        }

        public void PullCutScenes()
        {
            PullData<CutScene>($"/unit/cutscene/")
                .ForEach(item => CutScenes[item.ID] = item);
        }

        public void PullObjectInfos()
        {
            PullData<ObjectInfo>($"/unit/objectinfo/")
                .ForEach(item => ObjectInfos[item.ID] = item);
        }

        public void PullSounds()
        {
            PullData<Sound>($"/unit/sound/")
                .ForEach(item => Sounds[item.ID] = item);
        }

        public void PullFDSFiles()
        {
            PullData<FDSFile>($"/unit/fdsfile/")
                .ForEach(item => FDSFiles[item.ID] = item);
        }

        public void PullFDSs()
        {
            PullData<FDS>($"/unit/fds/")
                .ForEach(item => FDSs[item.ID] = item);
        }

        public void PullXREvents()
        {
            PullData<XREvent>($"/unit/xrevent/")
                .ForEach(item => XREvents[item.ID] = item);
        }

        public void PullEvaluationActions()
        {
            PullData<EvaluationAction>($"/unit/evaluationaction/")
                .ForEach(item => EvaluationActions[item.ID] = item);
        }

        public void PullEvaluations()
        {
            PullData<Evaluation>($"/unit/evaluation/")
                .ForEach(item => Evaluations[item.ID] = item);
        }

        public void PullSeparatedScenarios()
        {
            PullData<SeparatedScenario>($"/unit/separatedscenario/")
                .ForEach(item => SeparatedScenarios[item.ID] = item);
        }

        public void PullCombinedScenarios()
        {
            PullData<CombinedScenario>($"/unit/combinedscenario/")
                .ForEach(item => CombinedScenarios[item.ID] = item);
        }
        #endregion

        #region Push Data
        public async Task PushTransforms()
        {
            foreach (var transform in Transforms)
            {
                await _networkManager.PatchRequestAPIAsync($"/unit/transform/{transform.Key}/", JsonConvert.SerializeObject(transform.Value));
            }
        }

        public async Task PushInteractionPoints()
        {
            foreach (var interactionPoint in InteractionPoints)
            {
                await _networkManager.PatchRequestAPIAsync($"/unit/interactionpoint/{interactionPoint.Key}/", JsonConvert.SerializeObject(interactionPoint.Value));
            }
        }

        public async Task PushCutScenes()
        {
            foreach (var cutscene in CutScenes)
            {
                await _networkManager.PatchRequestAPIAsync($"/unit/cutscene/{cutscene.Key}/", JsonConvert.SerializeObject(cutscene.Value));
            }
        }

        public async Task PushObjectInfos()
        {
            foreach (var objectInfo in ObjectInfos)
            {
                await _networkManager.PatchRequestAPIAsync($"/unit/objectinfo/{objectInfo.Key}/", JsonConvert.SerializeObject(objectInfo.Value));
            }
        }

        public async Task PushSounds()
        {
            foreach (var sound in Sounds)
            {
                await _networkManager.PatchRequestAPIAsync($"/unit/sound/{sound.Key}/", JsonConvert.SerializeObject(sound.Value));
            }
        }

        public async Task PushFDSFiles()
        {
            foreach (var fdsFile in FDSFiles)
            {
                await _networkManager.PatchRequestAPIAsync($"/unit/fdsfile/{fdsFile.Key}/", JsonConvert.SerializeObject(fdsFile.Value));
            }
        }

        public async Task PushFDSs()
        {
            foreach (var fds in FDSs)
            {
                await _networkManager.PatchRequestAPIAsync($"/unit/fds/{fds.Key}/", JsonConvert.SerializeObject(fds.Value));
            }
        }

        public async Task PushXREvents()
        {
            foreach (var xrEvent in XREvents)
            {
                await _networkManager.PatchRequestAPIAsync($"/unit/xrevent/{xrEvent.Key}/", JsonConvert.SerializeObject(xrEvent.Value));
            }
        }

        public async Task PushEvaluationActions()
        {
            foreach (var evaluationAction in EvaluationActions)
            {
                await _networkManager.PatchRequestAPIAsync($"/unit/evaluationaction/{evaluationAction.Key}/", JsonConvert.SerializeObject(evaluationAction.Value));
            }
        }

        public async Task PushEvaluations()
        {
            foreach (var evaluation in Evaluations)
            {
                await _networkManager.PatchRequestAPIAsync($"/unit/evaluation/{evaluation.Key}/", JsonConvert.SerializeObject(evaluation.Value));
            }
        }

        public async Task PushSeparatedScenarios()
        {
            foreach (var separatedScenario in SeparatedScenarios)
            {
                await _networkManager.PatchRequestAPIAsync($"/unit/separatedscenario/{separatedScenario.Key}/", JsonConvert.SerializeObject(separatedScenario.Value));
            }
        }

        public async Task PushCombinedScenarios()
        {
            foreach (var combinedScenario in CombinedScenarios)
            {
                await _networkManager.PatchRequestAPIAsync($"/unit/combinedscenario/{combinedScenario.Key}/", JsonConvert.SerializeObject(combinedScenario.Value));
            }
        }
        #endregion

    }
}
