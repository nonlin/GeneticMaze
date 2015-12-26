using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Threading;

public class MazeManager : MonoBehaviour {

    //population of Genomes
    private List<SGenome> m_listGenomes = new List<SGenome>();
    //private SGenome[] m_aGenomes = new SGenome[100];
    //size of population
    [SerializeField] private int m_iPopsize = 140;
    [SerializeField] private double m_dCrossoverRate = 0.7f;
    [SerializeField] private double m_dMutationRate = 0.001f;
    //bits per chromosome
    [SerializeField] private int m_iChromoLength = 70;
    //bits per gene
    private int m_iGeneLength = 2;//10, 11, etc..
    private int m_iFittestGenome;
    private double m_dBestFitnessScore = 0.0f;
    private double m_dTotalFitnessScore;
    private int m_iGeneration = 0;
    //run in progress?
    private bool m_bBusy = false;
    //Maze Map 
    /*private int[,] MazeMap =
        { 
            {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
            {1, 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1},
            {8, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1},
            {1, 0, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 0, 0, 1},
            {1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 1, 0, 1},
            {1, 1, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 1, 0, 1},
            {1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 1, 1, 0, 1},
            {1, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 5},
            {1, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1},
            {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}
        };*/
    private List<GameObject> MazePieces = new List<GameObject>();
    //Maze Variables
    public int m_iMapWidth = 20;
    public int m_iMapHeight = 20;
    private int[,] MazeMap;
    //index the array which is the start point
    int m_iStartX;
    int m_iStartY;
    //finish point
    int m_iEndX;
    int m_iEndY;
    //Unity Objects
    public GameObject Empty;
    public GameObject Wall;
    public GameObject Begin;
    public GameObject End;
    public Canvas canvas;
    public Text infoText;
    public Text FitnessText;
    private static int offset = 100;
    private int CubeSize = 25;
    private Vector2 CurrentPos = new Vector2(offset, offset);
    Vector2 StartPoint;
    Vector2 EndPoint;
    System.Random r = new System.Random();
    public Thread t1;
    public string GenerationText;
    public string CurrentFittnessText;
    // Use this for initialization
    void Start ()
    {
        GenerateMaze();
        MakeMaze();
      
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (m_bBusy)
        {
            //Run();
            //Color Gray Squares
            for (int y = 0; y < m_iMapHeight; y++)
            {
                for (int x = 0; x < m_iMapWidth; x++)
                {
                    if (MazeMap[y, x] == -1)
                    {
                        for (int i = 0; i < MazePieces.Count; i++)
                        {
                            if (MazePieces[i].GetComponent<MazePieceIdentity>().x == x && MazePieces[i].GetComponent<MazePieceIdentity>().y == y)
                            {
                                MazePieces[i].GetComponent<Image>().color = Color.gray;
                            }
                        }
                    }
                }
            }
            //ResetSquares();
            //Update GUI Text
            infoText.text = GenerationText;
            FitnessText.text = CurrentFittnessText;
        }
    }

    public void Run()
    {
        t1 = new Thread(Epoch);
        //Create first population
        if (m_bBusy != true)
        {
            CreateStartPopulation();
            Debug.Log("Starting");
            m_bBusy = true;
        }
        if (!t1.IsAlive)
            t1.Start();
        //Epoch();
        //Debug.Log(m_iGeneration);
        
    }

    public void GenerateMaze()
    { 
        MazeMap = new int[m_iMapWidth, m_iMapHeight];
        for (int y = 0; y < m_iMapHeight; y++)
        {
            for (int x = 0; x < m_iMapWidth; x++)
            {
                //Make block of walls
                MazeMap[y, x] = 1;
            }
        }
        //Randomly Set a start on left
        StartPoint.y = r.Next(0, m_iMapHeight);
        StartPoint.x = 0;
        MazeMap[(int)StartPoint.y, (int)StartPoint.x] = 5;
        //Randomly Set a finish on right
        EndPoint.y = r.Next(0, m_iMapHeight);
        EndPoint.x = m_iMapHeight - 1;
        MazeMap[(int)EndPoint.y, (int) EndPoint.x ] = 8;
        //Rescurisvely chizle through walls
        recursion((int)StartPoint.y, (int)StartPoint.x);

    }


    public void recursion(int y, int x)
    {
        // 4 random directions
        int[] randDirs = generateRandomDirections();
        // Examine each direction
        for (int i = 0; i < randDirs.Count(); i++)
        {
            //Origon is bottom left corner
            switch (randDirs[i])
            {
                case 1: // Down
                        //　Whether 2 cells down is out or not
                    if (y - 2 <= 0)
                        continue;
                    if (MazeMap[y - 2, x] != 0)
                    {
                        MazeMap[y - 2,x] = 0;
                        MazeMap[y - 1,x] = 0;
                        recursion(y - 2, x);
                    }
                    break;
                case 2: // Right
                        // Whether 2 cells to the right is out or not
                    if (x + 2 >= m_iMapWidth - 1)
                        continue;
                    if (MazeMap[y,x + 2] != 0)
                    {
                        MazeMap[y,x + 2] = 0;
                        MazeMap[y,x + 1] = 0;
                        recursion(y, x + 2);
                    }
                    break;
                case 3: // Up
                        // Whether 2 cells Up is out or not
                    if (y + 2 >= m_iMapHeight - 1)
                        continue;
                    if (MazeMap[y + 2,x] != 0)
                    {
                        MazeMap[y + 2,x] = 0;
                        MazeMap[y + 1,x] = 0;
                        recursion(y + 2, x);
                    }
                    break;
                case 4: // Left
                        // Whether 2 cells to the left is out or not
                    if (x - 2 <= 0)
                        continue;
                    if (MazeMap[y,x - 2] != 0)
                    {
                        MazeMap[y,x - 2] = 0;
                        MazeMap[y,x - 1] = 0;
                        recursion(y, x - 2);
                    }
                    break;
            }
        }

    }

    /**
    * Generate an array with random directions 1-4
    * @return Array containing 4 directions in random order
    */
    public int[] generateRandomDirections()
    {
        List<int> randoms = new List<int>();

        for (int i = 0; i < 4; i++)
        {
            randoms.Add(r.Next(1, 5));
        }
            

        return randoms.ToArray();
    }

    void MakeMaze()
    {
        for (int y = 0; y < m_iMapHeight; y++)
        {
            for (int x = 0; x < m_iMapWidth; x++)
            {
                //Debug.Log(x + "," + y);
                if (MazeMap[y, x] == 1)
                {
                    GameObject MazePiece = Instantiate(Wall, CurrentPos, Quaternion.identity) as GameObject;
                    MazePiece.transform.SetParent(canvas.transform);
                    MazePiece.GetComponent<MazePieceIdentity>().x = x;
                    MazePiece.GetComponent<MazePieceIdentity>().y = y;
                    MazePiece.GetComponent<MazePieceIdentity>().type = 1;
                    MazePieces.Add(MazePiece);
                }
                else if (MazeMap[y, x] == 0)
                {
                    GameObject MazePiece = Instantiate(Empty, CurrentPos, Quaternion.identity) as GameObject;
                    MazePiece.transform.SetParent(canvas.transform);
                    MazePiece.GetComponent<MazePieceIdentity>().x = x;
                    MazePiece.GetComponent<MazePieceIdentity>().y = y;
                    MazePiece.GetComponent<MazePieceIdentity>().type = 0;
                    MazePieces.Add(MazePiece);
                }
                else if (MazeMap[y, x] == 5)
                {
                    GameObject MazePiece = Instantiate(Begin, CurrentPos, Quaternion.identity) as GameObject;
                    MazePiece.transform.SetParent(canvas.transform);
                    m_iStartX = x;
                    m_iStartY = y;
                    MazePiece.GetComponent<MazePieceIdentity>().x = x;
                    MazePiece.GetComponent<MazePieceIdentity>().y = y;
                    MazePiece.GetComponent<MazePieceIdentity>().type = 5;
                    MazePieces.Add(MazePiece);
                }
                else if (MazeMap[y, x] == 8)
                {
                    GameObject MazePiece = Instantiate(End, CurrentPos, Quaternion.identity) as GameObject;
                    MazePiece.transform.SetParent(canvas.transform);
                    m_iEndX = x;
                    m_iEndY = y;
                    MazePiece.GetComponent<MazePieceIdentity>().x = x;
                    MazePiece.GetComponent<MazePieceIdentity>().y = y;
                    MazePiece.GetComponent<MazePieceIdentity>().type = 8;
                    MazePieces.Add(MazePiece);
                }
                CurrentPos.x += CubeSize;
            }
            //Have to move back to offset like a fax machine
            CurrentPos.x = offset;
            CurrentPos.y += CubeSize;
        }
    }

    private double TestRoute( List<int> listPath)
    {
        int posX = m_iStartX;
        int posY = m_iStartY;

        for (int dir = 0; dir < listPath.Count(); ++dir)
        {
            int NextDir = listPath[dir];
            switch (NextDir)
            {
                case 0: //North   

                    //check within bounds and that we can move   
                    if (((posY - 1) < 0) || (MazeMap[posY - 1, posX] == 1))
                    {
                        break;
                    }

                    else
                    {
                        posY -= 1;
                    }

                    break;

                case 1: //South   

                    //check within bounds and that we can move   
                    if (((posY + 1) >= m_iMapHeight) || (MazeMap[posY + 1, posX] == 1))
                    {
                        break;
                    }

                    else
                    {
                        posY += 1;
                    }

                    break;

                case 2: //East   

                    //check within bounds and that we can move   
                    if (((posX + 1) >= m_iMapWidth) || (MazeMap[posY, posX + 1] == 1))
                    {
                        break;
                    }

                    else
                    {
                        posX += 1;
                    }

                    break;

                case 3: //West   

                    //check within bounds and that we can move   
                    if (((posX - 1) < 0) || (MazeMap[posY, posX - 1] == 1))
                    {
                        break;
                    }

                    else
                    {
                        posX -= 1;
                    }

                    break;

            }//end switch   
            MazeMap[posY, posX] = -1;
            //mark the route in the memory array
           /* for (int i = 0; i < MazePieces.Count; i++)
            {
                if (MazePieces[i].GetComponent<MazePieceIdentity>().x == posX && MazePieces[i].GetComponent<MazePieceIdentity>().y == posY)
                {
                    MazePieces[i].GetComponent<Image>().color = Color.gray;
                }
            }   */

        }//next direction   

        //now we know the finish point of Bobs journey, let's assign   
        //a fitness score which is proportional to his distance from   
        //the exit   

        int DiffX = Mathf.Abs(posX - m_iEndX);
        int DiffY = Mathf.Abs(posY - m_iEndY);

        //we add the one to ensure we never divide by zero. Therefore   
        //a solution has been found when this return value = 1   
        return 1 / (double)(DiffX + DiffY + 1);
    }

    private void CreateStartPopulation()
    {
        //clear existing poplulation
        if (m_listGenomes.Count() > 0)
        {
            m_listGenomes.Clear();
        }

        for (int i = 0; i < m_iPopsize; i++)
        {
            m_listGenomes.Add(new SGenome(m_iChromoLength));
            //Debug.Log(m_listGenomes[i].listBits.Count());
            /*string test = "";
            for (int k = 0; k < m_listGenomes[i].listBits.Count(); k++)
            {
                test = test + "-" + m_listGenomes[i].listBits[k];

            }
            Debug.Log(test);*/
        }

        //rest all variables
        m_iGeneration = 0;
        m_iFittestGenome = 0;
        m_dBestFitnessScore = 0;
        m_dTotalFitnessScore = 0;
    }

    private void Mutate( List<int> listBits)
    {
        //Mutate AKA Flip bits
        for (int curBit = 0; curBit < listBits.Count(); curBit++)
        {
            //prob to flip bit
            if (r.NextDouble() < m_dMutationRate)
            {
                //flip the bit
                listBits[curBit] = listBits[curBit] == 1 ? 0 : 1;
            }
        }//do next bit
    }

    private void Crossover( List<int> mum,  List<int> dad,  List<int> baby1,  List<int> baby2)
    {

        //.3 chance of not crossing over
        if ((r.NextDouble() > m_dCrossoverRate || (mum == dad)))
        {
            baby1 = mum;
            baby2 = dad;

            return;
        }
        //if we want to crossover and didn't return...
        //pick random point across chromosom to split
        int cp = r.Next(0, m_iChromoLength + 1);

        //from start to cp, add mum and dad stuff
        for (int i = 0; i < cp; i++)
        {
            baby1.Add(mum[i]);
            baby2.Add(dad[i]);
        }
        //from cp to end add dad and mum stuff,notice the swap of mum and dad, each baby needs genes from both mum and dad :) 
        for (int i = cp; i < m_iChromoLength; i++)
        {
            baby1.Add(dad[i]);
            baby2.Add(mum[i]);
        }
    }

    private SGenome RouletteWheelSelection()
    {
        double fslice = r.NextDouble() * m_dTotalFitnessScore;

        double cfTotal = 0;
        int SelectedGenome = 0;
        for (int i = 0; i < m_iPopsize; ++i)
        {
            cfTotal += m_listGenomes[i].dFitness;
            if (cfTotal > fslice)
            {
                SelectedGenome = i;
                break;
            }

        }

        return m_listGenomes[SelectedGenome];
    }

    private void UpdateFitnessScores()
    {
        m_iFittestGenome = 0;
        m_dBestFitnessScore = 0;
        m_dTotalFitnessScore = 0;
        //ResetSquares();
        //Update Fitness Score and keep a check on the fittest so far
        for (int i = 0; i < m_iPopsize; ++i)
        {
            //decode each genomes chromosome into a vector of directions   
            List<int> listDirections = Decode(m_listGenomes[i].listBits);

            //get it's fitness score   
            m_listGenomes[i].dFitness = TestRoute(listDirections);
            CurrentFittnessText = "Current Genom Fitness: " + m_listGenomes[i].dFitness.ToString();
            //FitnessText.text = "Current Genom Fitness: " + m_listGenomes[i].dFitness.ToString();
            //update total   
            m_dTotalFitnessScore += m_listGenomes[i].dFitness;

            //if this is the fittest genome found so far, store results   
            if (m_listGenomes[i].dFitness > m_dBestFitnessScore)
            {
                m_dBestFitnessScore = m_listGenomes[i].dFitness;

                m_iFittestGenome = i;

                //Has Bob found the exit?   
                if (m_listGenomes[i].dFitness >= 1)
                {
                    //is so, stop the run   
                    m_bBusy = false;
                    //Debug.Log("End Found");
                }
            }
     
        }//next genom

    }

    private List<int> Decode( List<int> bits)
    {
        List<int> directions = new List<int>();
        //step through chromosome a gene at a time
        for (int gene = 0; gene < bits.Count(); gene += m_iGeneLength)
        {
            //get the gene at this position
            List<int> ThisGene = new List<int>();
            for (int bit = 0; bit < m_iGeneLength; ++bit)
            {
                ThisGene.Add(bits[gene + bit]);
            }

            //convert to decimal and add to teh list of directions
            directions.Add(BitToInt(ThisGene));
        }

        return directions;
    }
    //Converts the two bits from binary to decimal and returns the value
    private int BitToInt( List<int> list)
    {
        int val = 0;
        int multiplier = 1;

        for (int cBit = list.Count(); cBit > 0; cBit--)
        {
            val += list[cBit - 1] * multiplier;
            multiplier *= 2;
        }

        return val;
    }

    public void Epoch()
    {
        while (m_bBusy) { 
            UpdateFitnessScores();
            //Now to create a new population
            int NewBabies = 0;

            //create some storage for baby genomes
            List<SGenome> listBabyGenomes = new List<SGenome>();

            while (NewBabies < m_iPopsize)
            {
                //Select 2 parents
                SGenome mum = RouletteWheelSelection();
                SGenome dad = RouletteWheelSelection();

                //operator - crossover
                SGenome baby1 = new SGenome(m_iChromoLength);
                SGenome baby2 = new SGenome(m_iChromoLength);
                Crossover(mum.listBits, dad.listBits, baby1.listBits, baby2.listBits);

                //operator - mutate
                Mutate(baby1.listBits);
                Mutate(baby2.listBits);

                //add to new population
                listBabyGenomes.Add(baby1);
                listBabyGenomes.Add(baby2);

                NewBabies += 2;

            }
            //Copy babies back into starter population
            m_listGenomes = listBabyGenomes;
            //increment the genration counter
            ++m_iGeneration;
            GenerationText = "Generation: " + m_iGeneration;
            //infoText.text = "Generation: " + m_iGeneration;
        }
    }

    //accessor methods
    public int Generation() { return m_iGeneration; }
    public int GetFittest() { return m_iFittestGenome; }
    public bool Started() { return m_bBusy; }
    public void Stop()
    {
        m_bBusy = false;
        ClearBobsPath();
        ResetSquares();
    }

    public void ResetSquares()
    {
        //Rest any space colors
        for (int i = 0; i < MazePieces.Count; i++)
        {
            if (MazePieces[i].GetComponent<MazePieceIdentity>().type == 0)
            {
                MazePieces[i].GetComponent<Image>().color = Color.white;
            }
            else if (MazePieces[i].GetComponent<MazePieceIdentity>().type == 5)
            {
                MazePieces[i].GetComponent<Image>().color = Color.green;
            }
            else if (MazePieces[i].GetComponent<MazePieceIdentity>().type == 8)
            {
                MazePieces[i].GetComponent<Image>().color = Color.red;
            }
        }
    }

    public void ClearBobsPath()
    {
        for (int y = 0; y < m_iMapHeight; y++)
        {
            for (int x = 0; x < m_iMapWidth; x++)
            {
                if (MazeMap[y, x] == -1)
                {
                    MazeMap[y, x] = 0;
                }
            }
        }
    }

    public void CreateNewMaze()
    {
        //Just load scene again
        SceneManager.LoadScene("MazeScene");
    }

    void OnApplicationQuit()
    {
        if(t1 != null)
            if (t1.IsAlive)
            {
                t1.Abort();
            }
    }
}
