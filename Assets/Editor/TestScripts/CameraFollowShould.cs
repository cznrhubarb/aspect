using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tests
{
    public class CameraFollowShould
    {
        private static IEnumerable<TestCaseData> CenterPositionData
        {
            get
            {
                yield return new TestCaseData(new Vector2(0, 0));
                yield return new TestCaseData(new Vector2(3, 1));
            }
        }
        [TestCaseSource("CenterPositionData")]
        public void CenterOnThePlayer(Vector2 playerPosition)
        {
            var player = new GameObject();
            player.name = "Player (Test)";
            player.transform.position = playerPosition;
            var follower = new CameraFollow(player);
            Assert.AreEqual(follower.Position, (Vector2)player.transform.position);
        }

        private static IEnumerable<TestCaseData> MovePositionData
        {
            get
            {
                yield return new TestCaseData(new Vector2(0, 0), new Vector2(3, 4));
                yield return new TestCaseData(new Vector2(3, 1), new Vector2(0, 0));
            }
        }
        [TestCaseSource("MovePositionData")]
        public void MoveWithThePlayer(Vector2 startPosition, Vector2 endPosition)
        {
            var player = new GameObject();
            player.name = "Player (Test)";
            player.transform.position = startPosition;
            var follower = new CameraFollow(player);
            player.transform.position = endPosition;
            follower.Update(100);
            Assert.AreEqual(follower.Position, endPosition);
        }
    }
}