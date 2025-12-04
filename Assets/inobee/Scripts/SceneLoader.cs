using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EmoteOrchestra.Core
{
    public interface ISceneLoader
    {
        Task LoadSingleAsync(string sceneName, CancellationToken ct);
        Task LoadAdditiveAsync(string sceneName, CancellationToken ct);
    }

    public sealed class SceneLoader : MonoBehaviour, ISceneLoader
    {
        private bool _isLoading;

        private void Awake()
        {
            ServiceLocator.Register<ISceneLoader>(this);
        }

        public async Task LoadSingleAsync(string sceneName, CancellationToken ct)
        {
            if (_isLoading) return;
            _isLoading = true;
            try
            {
                AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
                op.allowSceneActivation = true;
                while (!op.isDone)
                {
                    if (ct.IsCancellationRequested) break;
                    await Task.Yield();
                }
            }
            finally
            {
                _isLoading = false;
            }
        }

        public async Task LoadAdditiveAsync(string sceneName, CancellationToken ct)
        {
            if (_isLoading) return;
            _isLoading = true;
            try
            {
                AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                op.allowSceneActivation = true;
                while (!op.isDone)
                {
                    if (ct.IsCancellationRequested) break;
                    await Task.Yield();
                }
            }
            finally
            {
                _isLoading = false;
            }
        }
    }
}


