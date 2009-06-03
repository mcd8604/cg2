using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;

namespace Assignment2
{
    class Util
    {
        private static char[] seperators = { ' ' };

        public static List<KeyFrame> LoadKeyFrames(string inFile) 
        {
            List<KeyFrame> keyFrames = new List<KeyFrame>();
            
            if(File.Exists(inFile))
            {
                string[] lines = File.ReadAllLines(inFile);
                foreach (string s in lines)
                {
                    string[] line = s.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
                    if (line.Length == 8)
                    {
                        KeyFrame keyFrame = new KeyFrame();
                        keyFrame.time = float.Parse(line[0]);
                        keyFrame.position = new Vector3(
                            float.Parse(line[1]), 
                            float.Parse(line[2]), 
                            float.Parse(line[3]));
                        keyFrame.rotation = Quaternion.CreateFromAxisAngle(
                            new Vector3(
                                float.Parse(line[4]), 
                                float.Parse(line[5]), 
                                float.Parse(line[6])), 
                            MathHelper.ToRadians(float.Parse(line[7])));
                        keyFrames.Add(keyFrame);
                    }
                }
            }
            
            return keyFrames;
        }
    }
}
