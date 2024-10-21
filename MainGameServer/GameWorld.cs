

namespace MainGameServer
{
    internal class GameWorld
    {
        float playerMoveSpeed=4;
        float playerPosX = 400;
        float playerPosY = 240;
        int snapShotId = 0;

        public GameWorld()
        {
        }
        public void UpdateBallMovement(MovementUpdate mov)
        {
            if (mov.Moveleft)
            {
                playerPosX -= 1 * playerMoveSpeed;
            }
            else
            {
                playerPosX += 1 * playerMoveSpeed;
            }
            snapShotId = mov.SequenceNumber;
        }
        public SnapShot GetWorldStateSnapShot()
        {
            return new SnapShot() { ballPosY = playerPosY, ballPosX = playerPosX, SnapSeqId = snapShotId };
        }
    }
}
