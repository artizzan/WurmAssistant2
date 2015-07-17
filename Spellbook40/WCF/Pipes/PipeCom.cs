using System;
using System.ServiceModel;

namespace Aldurcraft.Spellbook40.WCF.Pipes
{
    public class PipeCom : IDisposable
    {
        private readonly string networkId;
        private readonly string channelId;
        private ServiceHost serviceHost;
        private PipeProxy serverProxy;
        IPipeProxy clientProxy;

        private bool initialized = false;

        public PipeCom(string networkId, string channelId)
        {
            this.networkId = networkId;
            this.channelId = channelId;
        }

        public void Send(string messageId, object obj)
        {
            clientProxy.Send(messageId, obj);
        }

        public bool TrySend(string messageId, object obj)
        {
            try
            {
                Send(messageId, obj);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public event EventHandler<PipeComEventArgs> MessageReceived;

        internal void OnMessageReceived(string messageId, object o)
        {
            var eh = MessageReceived;
            if (eh != null)
            {
                eh(this, new PipeComEventArgs(messageId, o));
            }
        }

        public void LoginAsEndpointAlpha()
        {
            OpenServer(PipeComEndPoint.Alpha);
            OpenClient(PipeComEndPoint.Beta);

            initialized = true;
        }

        public void LoginAsEndpointBeta()
        {
            OpenServer(PipeComEndPoint.Beta);
            OpenClient(PipeComEndPoint.Alpha);

            initialized = true;
        }

        public void LoginAsAlphaClient()
        {
            OpenClient(PipeComEndPoint.Alpha);

            initialized = true;
        }

        public void LoginAsBetaClient()
        {
            OpenClient(PipeComEndPoint.Beta);

            initialized = true;
        }

        private void OpenServer(PipeComEndPoint pipeComEndPoint)
        {
            ValidateNotInitialized();

            if (serviceHost == null)
            {
                serverProxy = new PipeProxy { PipeCom = this };

                serviceHost = new ServiceHost(
                    serverProxy,
                    new Uri[]
                    {
                        new Uri("net.pipe://localhost/"+networkId+"/"+channelId+"/"
                            + (pipeComEndPoint == PipeComEndPoint.Alpha ? "EndPointA" : "EndPointB") + "/")
                    });

                serviceHost.AddServiceEndpoint(typeof(IPipeProxy),
                    new NetNamedPipeBinding(), string.Empty);

                serviceHost.Open();
            }
            else
            {
                throw new InvalidOperationException("host already started");
            }
        }

        private void OpenClient(PipeComEndPoint pipeComEndPoint)
        {
            ValidateNotInitialized();

            if (clientProxy == null)
            {
                var pipeFactory =
                    new ChannelFactory<IPipeProxy>(
                        new NetNamedPipeBinding(),
                        new EndpointAddress(
                            "net.pipe://localhost/" + networkId + "/" + channelId + "/" +
                            (pipeComEndPoint == PipeComEndPoint.Alpha ? "EndPointA" : "EndPointB")));

                clientProxy = pipeFactory.CreateChannel();
            }
            else
            {
                throw new InvalidOperationException("proxy already opened");
            }
        }

        void ValidateNotInitialized()
        {
            if (initialized) throw new InvalidOperationException("this instance has already been initialized");
        }

        public void Dispose()
        {
            if (serverProxy != null)
            {
                serverProxy.PipeCom = null;
            }
            if (serviceHost != null)
            {
                ((IDisposable) serviceHost).Dispose();
            }
        }
        enum PipeComEndPoint { Alpha, Beta }
    }

    [ServiceContract]
    internal interface IPipeProxy
    {
        [OperationContract]
        void Send(string messageId, object o);
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    internal class PipeProxy : IPipeProxy
    {
        internal PipeCom PipeCom { get; set; }

        public void Send(string messageId, object o)
        {
            PipeCom.OnMessageReceived(messageId, o);
        }
    }

    public class PipeComEventArgs : EventArgs
    {
        public string MessageId { get; private set; }
        public object Data { get; private set; }

        public PipeComEventArgs(string messageId, object data)
        {
            MessageId = messageId;
            Data = data;
        }
    }
}
