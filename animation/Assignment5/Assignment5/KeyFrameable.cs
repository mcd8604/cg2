using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Assignment5
{
    class KeyFrameable : GameComponent
    {
        protected Vector3 position = Vector3.Zero;
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        private List<KeyFrame> keyFrames;
        public List<KeyFrame> KeyFrames
        {
            get { return keyFrames; }
            set 
            { 
                keyFrames = value;
                Reset();
            }
        }

        private int currentKeyFrameIndex;
        private int nextKeyFrameIndex;
        private double time;

        public KeyFrameable(Game game)
            : base(game) { }

        public override void Update(GameTime gameTime)
        {
            time += gameTime.ElapsedGameTime.TotalSeconds;

            if (time > keyFrames[nextKeyFrameIndex].time)
            {
                advanceKeyFrame();
            }

            float amount = (float)((time - keyFrames[currentKeyFrameIndex].time) / (keyFrames[nextKeyFrameIndex].time - keyFrames[currentKeyFrameIndex].time));

            position = Vector3.Lerp(keyFrames[currentKeyFrameIndex].position, keyFrames[nextKeyFrameIndex].position, amount);
            
            base.Update(gameTime);
        }

        /// <summary>
        /// Advances indices to the next KeyFrame. 
        /// Calls Reset() when nextKeyFrameIndex is at the end of the list. 
        /// </summary>
        private void advanceKeyFrame()
        {
            ++currentKeyFrameIndex;

            if (nextKeyFrameIndex < keyFrames.Count - 1)
            {
                ++nextKeyFrameIndex;
            }
            else
            {
                Reset();
            }
        }

        public void Reset()
        {
            time = 0;
            currentKeyFrameIndex = 0;
            nextKeyFrameIndex = 1;
        }
    }
}
