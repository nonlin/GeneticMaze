using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[System.Serializeable] 
public class SGenome {

    //Genetic Variables
    public List<int> listBits;
    public double dFitness;
    System.Random r = new System.Random();
    //Constructor
    public SGenome(int num_bits)
    {
        //init genome's fitness and chromosomes
        this.dFitness = 0;
        listBits = new List<int>();
        for (int i = 0; i < num_bits; ++i)
        {
            listBits.Add(r.Next(0, 2));
        }
    }
}
