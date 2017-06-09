using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class RoomScenes : MonoBehaviour {

    //单元格尺寸80*80
    const float cellSize = 80f;

    Canvas uiCanvas = null;

    //当前地图坐标点
    GameObject[,] pointObj = null;
    //当前地图坐标填充
    GameObject[,] paddingObj = null;
    //填充的图像
    Image[,] paddingImg = null;

    //手指位置标记
    GameObject flag = null;

    //地图所有单元格
    MapCell[,] AllMapCellArr = null;

    //所有路线映射 id--单条线路list
    Dictionary<int, List<MapCell>> pathDic = new Dictionary<int, List<MapCell>>();
    int currPath = -1;

    //是否开始检测手指拖动
    bool bBeginCheckTouchDrag = false;

    bool isPaddingAll = false;
    bool isPathSucess = false;

    /// <summary>
    /// 上一帧手指触摸的单元格
    /// </summary>
    int lastTouchRow = -1;
    int lastTouchCol = -1;


    void Awake() {
        
        LevelConfig.ReadLevelConfig();

        AllMapCellArr = new MapCell[LevelConfig.rowNum, LevelConfig.colNum];

        uiCanvas = transform.GetComponent<Canvas>();
        pointObj = new GameObject[LevelConfig.rowNum, LevelConfig.colNum];
        paddingObj = new GameObject[LevelConfig.rowNum, LevelConfig.colNum];
        paddingImg = new Image[LevelConfig.rowNum, LevelConfig.colNum];

        ResetMap();
    }

	// Use this for initialization
	void Start () {

        InitMap();
        InitLayout();
	}
	
	// Update is called once per frame
	void Update () {
        MakeConnect();
    }

    /// <summary>
    /// 地图数据初始化
    /// </summary>
    void ResetMap() {
        //初始化地图所有单元格
        for (int i = 0; i < LevelConfig.rowNum; ++i)
        {
            for (int j = 0; j < LevelConfig.colNum; ++j)
            {
                AllMapCellArr[i, j] = new MapCell(i, j, true, null);
            }
        }
        //填充初始物体
        for (int i = 0; i < LevelConfig.cubeList.Count; ++i)
        {
            AllMapCellArr[LevelConfig.cubeList[i].row, LevelConfig.cubeList[i].col].cube = LevelConfig.cubeList[i];
            AllMapCellArr[LevelConfig.cubeList[i].row, LevelConfig.cubeList[i].col].canPadding = false;
            //路径字典，以id为key
            if (!pathDic.ContainsKey(LevelConfig.cubeList[i].id))
            {
                pathDic.Add(LevelConfig.cubeList[i].id, new List<MapCell>());
            }
        }
        //填充初始障碍物
        for (int i = 0; i < LevelConfig.obstacleList.Count; ++i)
        {
            AllMapCellArr[LevelConfig.obstacleList[i].row, LevelConfig.obstacleList[i].col].cube = LevelConfig.obstacleList[i];
            AllMapCellArr[LevelConfig.obstacleList[i].row, LevelConfig.obstacleList[i].col].canPadding = false;
        }
    }

    /// <summary>
    /// 地图界面规模初始化
    /// </summary>
    void InitMap()
    {
        //取中间行用
        for (int i = 0; i < LevelConfig.rowNum; ++i)
        {
            int row = (LevelConfig.RowMax - LevelConfig.rowNum) / 2 + (i + 1);
            transform.FindChild("map/row (" + row + ")").gameObject.SetActive(true);
            //取中间列用
            for (int j = 0; j < LevelConfig.colNum; ++j)
            {
                int col = (LevelConfig.ColMax - LevelConfig.colNum) / 2 + (j + 1);
                pointObj[i, j] = transform.FindChild("map/row (" + row + ")/point (" + col + ")").gameObject;
                pointObj[i, j].SetActive(true);

                paddingObj[i, j] = pointObj[i, j].transform.FindChild("Image").gameObject;
                paddingImg[i, j] = paddingObj[i, j].GetComponent<Image>();
                paddingObj[i, j].SetActive(false);
            }
            //关闭开头列
            for (int j = 0; j < (LevelConfig.ColMax - LevelConfig.colNum) / 2; ++j)
            {
                transform.FindChild("map/row (" + row + ")/point (" + (j + 1) + ")").gameObject.SetActive(false);
            }
            //关闭结尾列
            for (int j = LevelConfig.ColMax - (LevelConfig.ColMax - LevelConfig.colNum) / 2; j < LevelConfig.ColMax; ++j)
            {
                transform.FindChild("map/row (" + row + ")/point (" + (j + 1) + ")").gameObject.SetActive(false);
            }
        }
        //关闭开头行
        for (int i = 0; i < (LevelConfig.RowMax - LevelConfig.rowNum) / 2; ++i)
        {
            transform.FindChild("map/row (" + (i + 1) + ")").gameObject.SetActive(false);
        }
        //关闭结尾行
        for (int i = LevelConfig.RowMax - (LevelConfig.RowMax - LevelConfig.rowNum) / 2; i < LevelConfig.RowMax; ++i)
        {
            transform.FindChild("map/row (" + (i + 1) + ")").gameObject.SetActive(false);
        }

        flag = transform.FindChild("flag").gameObject;
        flag.SetActive(false);
    }

    /// <summary>
    /// 初始化物体和障碍摆放
    /// </summary>
    void InitLayout() {
        for (int i = 0; i < LevelConfig.cubeList.Count; ++i) {
            paddingObj[LevelConfig.cubeList[i].row, LevelConfig.cubeList[i].col].SetActive(true);
            paddingImg[LevelConfig.cubeList[i].row, LevelConfig.cubeList[i].col].sprite = 
                Resources.Load("Texs/" + LevelConfig.cubeList[i].id.ToString(), typeof(Sprite)) as Sprite;
        }

        for (int i = 0; i < LevelConfig.obstacleList.Count; ++i)
        {
            paddingObj[LevelConfig.obstacleList[i].row, LevelConfig.obstacleList[i].col].SetActive(true);
            paddingImg[LevelConfig.obstacleList[i].row, LevelConfig.obstacleList[i].col].sprite = 
                Resources.Load("Texs/" + LevelConfig.obstacleList[i].id.ToString(), typeof(Sprite)) as Sprite;
        }
    }

    /// <summary>
    /// 初始化各种技能道具
    /// </summary>
    void InitSkills() { 
        
    }

    //线路连接
    void MakeConnect() {
        if (IsTouchBegin()) {
            bBeginCheckTouchDrag = true;
        }
        if (bBeginCheckTouchDrag) {
            GetCellByPos();
        }
        if (IsTouchEnd()) {
            currPath = -1;
            bBeginCheckTouchDrag = false;
            flag.SetActive(false);
            CheckFinish();
        }
    }

    /// <summary>
    /// 检测是否完成全部链接
    /// </summary>
    void CheckFinish() {
        //看是否有没填充完的单元格
        for (int i = 0; i < LevelConfig.rowNum; ++i)
        {
            for (int j = 0; j < LevelConfig.colNum; ++j)
            {
                if (AllMapCellArr[i, j].cube == null) {
                    isPaddingAll = false;
                    return;
                }
            }
        }
        //都填充满了
        isPaddingAll = true;
        //看路线是否是合格的
        foreach (int k in pathDic.Keys) {
            for (int i = 0; i < pathDic[k].Count-1; ++i) {
                int offsetRow = Mathf.Abs(pathDic[k][i + 1].rowIndex - pathDic[k][i].rowIndex);
                int offsetCol = Mathf.Abs(pathDic[k][i + 1].colIndex - pathDic[k][i].colIndex);
                if ((offsetRow == 1 && offsetCol == 0) || (offsetRow == 0 && offsetCol == 1))
                {
                    Debug.Log("游戏结束，进入结算模块");
                }
            }
        }
    }

    //通过位置坐标获得单元格索引
    void GetCellByPos() {

        int rowTmp = -1;
        int colTmp = -1;

        //更新手指标识的位置
        Vector3 uiPos = uiCanvas.worldCamera.ScreenToWorldPoint(GetCurrTouchPos());
        flag.SetActive(true);
        flag.transform.position = uiPos;
        flag.transform.localPosition = new Vector3(flag.transform.localPosition.x, flag.transform.localPosition.y, 0f);

        //计算有效位置行列索引
        Vector2 uiVec = new Vector2(flag.transform.localPosition.x, flag.transform.localPosition.y);
        if (uiVec.x < -cellSize * LevelConfig.rowNum / 2 || uiVec.x > cellSize * LevelConfig.rowNum / 2 ||
            uiVec.y < -cellSize * LevelConfig.colNum / 2 || uiVec.y > cellSize * LevelConfig.colNum / 2)
        {
            flag.SetActive(false);
            lastTouchRow = -1;
            lastTouchCol = -1;
            currPath = -1;
            return;
        }
        for (int i = 0; i < LevelConfig.rowNum; ++i) {
            if (uiVec.x >= (i - LevelConfig.rowNum / 2 - 0.5f) * cellSize && uiVec.x <= (i - LevelConfig.rowNum / 2 + 0.5f) * cellSize) {
                rowTmp = i;
                break;
            }
        }
        for (int i = 0; i < LevelConfig.colNum; ++i)
        {
            if (uiVec.y <= -1f*(i - LevelConfig.colNum / 2 - 0.5f) * cellSize && uiVec.y >= -1f*(i - LevelConfig.colNum / 2 + 0.5f) * cellSize)
            {
                colTmp = i;
                break;
            }
        }
        if (!rowTmp.Equals(-1) && !colTmp.Equals(-1)) {
            int offsetRow = Mathf.Abs(rowTmp - lastTouchRow);
            int offsetCol = Mathf.Abs(colTmp - lastTouchCol);
            //相邻单元格
            if ((offsetRow == 1 && offsetCol == 0) || (offsetRow == 0 && offsetCol == 1) || lastTouchRow == -1)
            {
                lastTouchRow = rowTmp;
                lastTouchCol = colTmp;
                //添加单元格至当前路径中
                Debug.Log("row, col:" + rowTmp + "," + colTmp);
                RefreshPath(rowTmp, colTmp);
            }
        }
    }

    /// <summary>
    /// 刷新路径
    /// </summary>
    void RefreshPath(int rowTmp, int colTmp)
    {
        if (AllMapCellArr[rowTmp, colTmp].canPadding)
        {
            if (currPath == -1)
            {//当前没有正在连接的路径
                if (AllMapCellArr[rowTmp, colTmp].cube != null)
                {
                    currPath = AllMapCellArr[rowTmp, colTmp].cube.id;
                    RefreshCurrPath(AllMapCellArr[rowTmp, colTmp].cube);
                }
            }
            else
            {//有则添加 
                Cube cube = new Cube();
                if (AllMapCellArr[rowTmp, colTmp].cube != null)
                {
                    cube = AllMapCellArr[rowTmp, colTmp].cube;
                    RefreshOtherPath(cube);
                }
                else
                {
                    cube.id = currPath;
                    cube.row = rowTmp;
                    cube.col = colTmp;
                    cube.padType = PadType.LINE;
                }
                MapCell mapCell = new MapCell(rowTmp, colTmp, true, cube);
                if (!pathDic[currPath].Contains(mapCell))
                {
                    pathDic[currPath].Add(mapCell);
                }
                else
                {
                    RefreshCurrPath(cube);
                }
            }
        }
        else
        {
            if (AllMapCellArr[rowTmp, colTmp].cube.padType == PadType.NORMAL)
            {
                if (currPath == -1)
                {
                    currPath = AllMapCellArr[rowTmp, colTmp].cube.id;
                    pathDic[currPath].Clear();
                }
                else {
                    lastTouchRow = -1;
                    lastTouchCol = -1;
                }
                MapCell mapCell = new MapCell(rowTmp, colTmp, false, AllMapCellArr[rowTmp, colTmp].cube);
                pathDic[currPath].Add(mapCell);
            }
            else if (AllMapCellArr[rowTmp, colTmp].cube.padType == PadType.OBSTACLE)
            {
                currPath = -1;
                lastTouchRow = -1;
                lastTouchCol = -1;
            }
        }
        if (lastTouchRow == -1 || lastTouchCol == -1) {
            currPath = -1;
        }
    }

    /// <summary>
    /// 刷新当前路径
    /// </summary>
    /// <param name="cube"></param>
    void RefreshCurrPath(Cube cube){
        for (int i = 0; i < pathDic[currPath].Count; ++i)
        {
            if ((pathDic[currPath][i].cube == cube) && (pathDic[currPath].Count - i>1))
            {
                pathDic[currPath].RemoveRange(i+1, pathDic[currPath].Count - i-1);
                break;
            }
        }
    }

    /// <summary>
    /// 刷新其他受影响的路线
    /// </summary>
    /// <param name="cube"></param>
    void RefreshOtherPath(Cube cube) {
        foreach(int k in pathDic.Keys) {
            if (k == currPath) { continue; }
            for (int i = 0; i < pathDic[k].Count; ++i)
            {
                if (pathDic[k][i].cube == cube)
                {
                    pathDic[k].RemoveRange(i, pathDic[k].Count);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 触屏开始
    /// </summary>
    /// <returns></returns>
    bool IsTouchBegin()
    {
        bool isbegin = false;
#if !UNITY_EDITOR && (UNITY_IOS  || UNITY_ANDROID)
        int tCount=Input.touchCount ;
        for (int i = 0; i < tCount; i++)
        {
            if (Input.GetTouch(i).phase == TouchPhase.Began)
            {
                isbegin = true;
                break;
            }
        }
#else
        isbegin = Input.GetMouseButtonDown(0);
#endif
        return isbegin;
    }

    /// <summary>
    /// 触屏结束
    /// </summary>
    /// <returns></returns>
    bool IsTouchEnd()
    {
        bool isend = false;
#if !UNITY_EDITOR && (UNITY_IOS  || UNITY_ANDROID)
        for (int c = 0; c < Input.touchCount; c++)
        {
            if (Input.GetTouch(c).phase == TouchPhase.Ended)
            {
                isend = true;
                break;
            }
        }
#else
        isend = Input.GetMouseButtonUp(0);
#endif
        return isend;
    }

    /// <summary>
    /// 获取当前触屏坐标
    /// </summary>
    /// <returns></returns>
    Vector2 GetCurrTouchPos()
    {
#if !UNITY_EDITOR && (UNITY_IOS  || UNITY_ANDROID)
        int touchCount=Input.touchCount;
        if(touchCount >0){
            for (int i = 0; i < touchCount; i++)
            {
                if (Input.GetTouch(i).phase == TouchPhase.Moved)
                {
                    return Input.GetTouch(i).position;
                }
            }
            return Input.GetTouch(touchCount - 1).position;
        }
        return Vector2.zero;
#else
        return Input.mousePosition;
#endif
    }
}
