using Client.GameObjects;
using MessagePack;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace Client
{
    public class Game1
    {
        private string _ip;
        private int _port;
        private List<GameObject> gameObjects = new List<GameObject>();
        public static bool USE_INTERPOLATION = true;
        public static bool USE_RECONCILIATION = true;
        public static bool USE_PREDICTION = true;
        public static int LATENCY = 250;
        private Player player;
        private TCPGameClient client;
        private bool isExiting = false;

        public Game1(string ip, int port)
        {
            _ip = ip;
            _port = port;
        }

        public void Initialize()
        {
            client = new TCPGameClient(OnDataReceived, _ip, _port);
            client.SendDataToServer(new JoinMessage());
            StartGame();
        }

        private void OnDataReceived(byte[] receivedData)
        {
            MessageType messageType = (MessageType)receivedData[0];
            byte[] dataToDeserialize = receivedData.Skip(1).ToArray();
            switch (messageType)
            {
                case MessageType.SnapShot:
                    SnapShot snap = MessagePackSerializer.Deserialize<SnapShot>(dataToDeserialize);
                    player.HandleSnapShot(snap);
                    break;
                case MessageType.JoinAnswer:
                    player.Owner = MessagePackSerializer.Deserialize<JoinAnswer>(dataToDeserialize).BallOwner;
                    break;
                case MessageType.UpdateTickRate:
                    player.SetTickRate(MessagePackSerializer.Deserialize<UpdateTickRate>(dataToDeserialize).TickRate);
                    break;
                default:
                    break;
            }
        }

        public void StartGame()
        {
            player = new Player(new Vector2(250, 250), client);
            gameObjects.Add(player);
            gameObjects.ForEach(x => x.Init());
        }

        public void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            GetState();

            if (HasBeenPressed(Keys.Add))
            {
                Debug.WriteLine("hello");
                LATENCY += 50;
            }
            if (HasBeenPressed(Keys.OemMinus))
            {
                LATENCY -= 50;
            }
            if (HasBeenPressed(Keys.P))
            {
                Debug.WriteLine("hello");
                USE_PREDICTION = !USE_PREDICTION;
            }
            if (HasBeenPressed(Keys.I))
            {
                USE_INTERPOLATION = !USE_INTERPOLATION;
            }
            if (HasBeenPressed(Keys.R))
            {
                USE_RECONCILIATION = !USE_RECONCILIATION;
            }

            gameObjects.ForEach(x => x.Update(gameTime));
        }

        private static KeyboardState currentKeyState;
        private static KeyboardState previousKeyState;

        public KeyboardState GetState()
        {
            previousKeyState = currentKeyState;
            currentKeyState = Keyboard.GetState();
            return currentKeyState;
        }

        public bool HasBeenPressed(Keys key)
        {
            return currentKeyState.IsKeyDown(key) && !previousKeyState.IsKeyDown(key);
        }

        public void Exit()
        {
            isExiting = true;
            // Add any additional cleanup logic here if necessary
        }

        public bool IsExiting()
        {
            return isExiting;
        }
    }
}
