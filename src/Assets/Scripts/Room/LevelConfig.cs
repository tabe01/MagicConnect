using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelConfig{

    public static string levelConfigPath = Application.streamingAssetsPath + "/Jsons/" + "Level1Config.json";

    //地图最大行列数
    public const int RowMax = 9;
    public const int ColMax = 9;

    //当前地图行列数,默认最大 (为了方便对称，最好只用奇数)
    public static int rowNum = RowMax; //RowMax
    public static int colNum = ColMax; //ColMax

    /// <summary>
    /// 初始摆放物体
    /// </summary>
    public static List<Cube> cubeList = new List<Cube>();

    //障碍物
    public static List<Cube> obstacleList = new List<Cube>();

    /// <summary>
    /// 读取关卡配置
    /// </summary>
    public static void ReadLevelConfig()
    {
        Hashtable tbl = Utils.ReadJsonFile(levelConfigPath);
        if (tbl != null && tbl.Count > 0)
        {
            rowNum = int.Parse(tbl["rowNum"].ToString());
            colNum = int.Parse(tbl["colNum"].ToString());

            //读取初始摆放(填充)物体
            ArrayList cubeArr = tbl["cubeData"] as ArrayList;
            foreach (Hashtable item in cubeArr) {
                ArrayList posArr = item["pos"] as ArrayList;
                foreach (Hashtable tmp in posArr) {
                    Cube cube = new Cube();
                    cube.id = int.Parse(item["cubeId"].ToString());
                    cube.row = int.Parse(tmp["row"].ToString());
                    cube.col = int.Parse(tmp["col"].ToString());
                    cube.padType = PadType.NORMAL;

                    cubeList.Add(cube);
                }
            }
            Debug.Log("cube count:"+cubeList.Count);

            //读取初始障碍物体
            ArrayList obstacleArr = tbl["obstacleData"] as ArrayList;
            foreach (Hashtable item in obstacleArr)
            {
                ArrayList posArr = item["pos"] as ArrayList;
                foreach (Hashtable tmp in posArr)
                {
                    Cube cube = new Cube();
                    cube.id = int.Parse(item["obstacleId"].ToString());
                    cube.row = int.Parse(tmp["row"].ToString());
                    cube.col = int.Parse(tmp["col"].ToString());
                    cube.padType = PadType.OBSTACLE;

                    obstacleList.Add(cube);
                }
            }
            Debug.Log("obstacle count:" + obstacleList.Count);
        }
    }
}
