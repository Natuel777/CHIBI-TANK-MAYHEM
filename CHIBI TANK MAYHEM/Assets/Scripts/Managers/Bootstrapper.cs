using UnityEngine;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Installers
{
    public sealed class Bootstrapper : MonoBehaviour, IServiceConsumer
    {
        public const string StartSceneKey = "Login";
        private SceneLoader _sceneLoader;
        [SerializeField] private AppInstaller _appInstaller;
        [SerializeField] private LoadingScreen _loadingScreen;

        private async void Start()
        {
            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, Application.exitCancellationToken);

            try
            {
                await BootstrapperAsync(cancellation.Token);
            }

            catch(OperationCanceledException) when (cancellation.IsCancellationRequested) {}

            catch(Exception exception)
            {
               _loadingScreen.SetStatus($"Error: {exception.Message}");
            }
        }

        private async Task BootstrapperAsync(CancellationToken cancellationToken)
        {
            //Lógica de carga de PlayerData

            _loadingScreen.SetStatus("Instalando servicios...");
            await _appInstaller.Install();
            _loadingScreen.SetStatus($"Servicios listos. Cargando {StartSceneKey}...");

            if(TryResolveService())
            {
                await _sceneLoader.LoadSceneAsync(StartSceneKey);
            }

            else _loadingScreen.SetStatus($"Error: No se pudo resolver el servicio SceneLoader.");
        }

        public bool TryResolveService()
        {
            if(ServiceLocator.Instance.TryGet<SceneLoader>(out _sceneLoader))
                return true;

            return false;
        }
    }   
}