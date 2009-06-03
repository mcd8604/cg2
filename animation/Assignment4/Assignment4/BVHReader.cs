using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;

namespace Assignment4
{
    class BVHReader
    {
        /// <summary>
        /// Reads a BVH file, populates the Nodes list and motion data.
        /// </summary>
        /// <param name="fileName">Name of the BVH file to read.</param>
        public void ReadFile(string fileName) 
        {
            if (!File.Exists(fileName)) return;

            string[] lines = File.ReadAllLines(fileName);

            int i = 0;

            if (lines[i] == "HIERARCHY")
            {
                i = readHierarchy(lines, i);
            }

            if (lines[++i] == "MOTION")
            {
                readMotion(lines, i);
            }

        }

        private static string[] seperator = new string[] { " ", "\t" };

        private List<BVHNode> nodes;
        public List<BVHNode> Nodes
        {
            get { return nodes; }
        }

        private int numFrames;
        public int NumFrames
        {
            get { return numFrames; }
            set { numFrames = value; }
        }

        private float frameTime;
        public float FrameTime
        {
            get { return frameTime; }
            set { frameTime = value; }
        }

        private float[][] frames;
        public float[][] Frames
        {
            get { return frames; }
            set { frames = value; }
        }

        /// <summary>
        /// Reads header section which describes the hierarchy and initial pose of the skeleton;
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private int readHierarchy(string[] lines, int i)
        {
            int depth = 0;

            nodes = new List<BVHNode>();
            BVHNode curNode = null;

            do
            {
                string[] line = lines[++i].Split(seperator, StringSplitOptions.RemoveEmptyEntries);
                if (line[0] == "{")
                {
                    // do nothing
                }
                else if (line[0] == "}")
                {
                    BVHChildNode childNode = curNode as BVHChildNode;
                    if (childNode != null)
                    {
                        curNode = childNode.Parent;
                    }
                    --depth;
                } 
                else if (line[0] == "ROOT")
                {
                    curNode = new BVHNode(line[1]);

                    nodes.Add(curNode);

                    ++depth;
                }
                else if (line[0] == "JOINT")
                {
                    BVHJoint joint = new BVHJoint(line[1], curNode);
                    curNode.Nodes.Add(joint);
                    curNode = joint;

                    ++depth;
                }
                else if (line[0] == "End")
                {
                    BVHEndSite end = new BVHEndSite(curNode);
                    curNode.Nodes.Add(end);
                    curNode = end;

                    ++depth;
                }
                else if (line[0] == "OFFSET")
                {
                    curNode.Offset = new Vector3(float.Parse(line[1]), float.Parse(line[2]), float.Parse(line[3]));
                }
                else if (line[0] == "CHANNELS")
                {
                    int numChannels = int.Parse(line[1]);
                    for (int c = 2; c < numChannels + 2; ++c)
                    {
                        Channel channel = Channel.None;
                        channel = (Channel)Enum.Parse(typeof(Channel), line[c]);
                        curNode.Channels |= channel;
                    }
                }
            } while (depth > 0);
            return i;
        }

        /// <summary>
        /// Reads data section which contains the motion data.
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="i"></param>
        private void readMotion(string[] lines, int i)
        {
            int frameNum = 0;
            while(i < lines.Length - 1) 
            {
                string[] line = lines[++i].Split(seperator, StringSplitOptions.RemoveEmptyEntries);
                
                if (line[0] == "Frames:")
                {
                    numFrames = int.Parse(line[1]);
                    frames = new float[numFrames][];
                }
                else if(line[0] == "Frame"  && line[1] == "Time:")
                {
                    frameTime = float.Parse(line[2]);
                }
                else
                {
                    //Each line is one sample of motion data. 
                    //The numbers appear in the order of the channel specifications as the skeleton hierarchy was parsed.
                    frames[frameNum] = new float[line.Length];
                    for (int j = 0; j < line.Length; ++j)
                        frames[frameNum][j] = float.Parse(line[j]);
                    ++frameNum;
                }
            }
        }
    }
}
