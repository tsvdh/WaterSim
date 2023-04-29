using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class WaterManager : MonoBehaviour
{
    public int ticksPerSecond;
    private float _timeSinceLastUpdate;
    
    private WaterBlock[,,] _waterBlocks;
    private LinkedList<Vector3Int> _waterBlockList;
    private bool[,,] _solidBlocks;

    public GameObject waterBlockPrefab;

    // Start is called before the first frame update
    private void Start()
    {
        _waterBlocks = new WaterBlock[100,100,100];
        _waterBlockList = new LinkedList<Vector3Int>();
        foreach (WaterBlock block in FindObjectsOfType<WaterBlock>())
        {
            Vector3 worldCoors = block.transform.position;
            var intCoors = new Vector3Int((int) worldCoors.x, (int) worldCoors.y, (int) worldCoors.z);
            intCoors = ToGridCoors(intCoors);
            _waterBlocks[intCoors.x, intCoors.y, intCoors.z] = block;
            _waterBlockList.AddLast(intCoors);
        }

        _solidBlocks = new bool[100, 100, 100];
        foreach (GameObject block in GameObject.FindGameObjectsWithTag("Solid"))
        {
            Vector3 worldCoors = block.transform.position;
            var intCoors = new Vector3Int((int) worldCoors.x, (int) worldCoors.y, (int) worldCoors.z);
            intCoors = ToGridCoors(intCoors);
            _solidBlocks[intCoors.x, intCoors.y, intCoors.z] = true;
        }
    }
    
    // Update is called once per frame
    private void Update()
    {
        float updateInterval = 1f / ticksPerSecond;
        _timeSinceLastUpdate += Time.deltaTime;
        
        if (_timeSinceLastUpdate >= updateInterval)
            _timeSinceLastUpdate = 0;
        else
            return;

        var waterBlockListCopy = new LinkedList<Vector3Int>();
        foreach (Vector3Int blockCoors in _waterBlockList)
        {
            waterBlockListCopy.AddLast(blockCoors);
        }

        foreach (Vector3Int intCoors in waterBlockListCopy)
        {
            Vector3Int downIntCoors = intCoors;
            downIntCoors.y--;

            float remaining = TransferWaterRatio(1, intCoors, downIntCoors);
            
            if (remaining == 0)
                continue;

            // build side coors
            var sides = new List<Vector3Int>(4);
            for (int dx = -1; dx <= 1; dx += 2)
            {
                for (int dy = -1; dy <= 1; dy += 2)
                {
                    Vector3Int sideIntCoors = intCoors;
                    sideIntCoors.x += dx;
                    sideIntCoors.y += dy;
                    if (!_solidBlocks[sideIntCoors.x, sideIntCoors.y, sideIntCoors.z])
                        sides.Add(sideIntCoors);
                }
            }

            sides.Sort((a, b) =>
            {
                float aAmount = _waterBlocks[a.x, a.y, a.z].Amount;
                float bAmount =  _waterBlocks[b.x, b.y, b.z].Amount;
                if (aAmount < bAmount) return -1;
                if (aAmount > bAmount) return 1;
                return 0;
            });
            
            foreach (Vector3Int side in sides)
            {
                float amountToTransfer;
                if (remaining < 0.01) 
                    amountToTransfer = remaining;
                else
                {
                    if (!_waterBlocks[side.x, side.y, side.z])
                        amountToTransfer = remaining / 2;
                    else
                    {
                        float diff = remaining - _waterBlocks[side.x, side.y, side.z].Amount;
                        amountToTransfer = diff > 0 ? diff / 2 : 0;
                    }
                }

                if (amountToTransfer > 0)
                    TransferWaterAmount(amountToTransfer, intCoors, side);

                remaining -= amountToTransfer;
            }
        }
    }
    
    // returns water not transferred
    private float TransferWaterRatio(float ratio, Vector3Int from, Vector3Int to)
    {
        WaterBlock fromBlock = _waterBlocks[from.x, from.y, from.z];

        float diff = fromBlock.Amount / ratio;

        return TransferWaterAmount(diff, from, to);
    }
    
    // returns water not transferred
    private float TransferWaterAmount(float amount, Vector3Int from, Vector3Int to)
    {
        float transferred;
        
        WaterBlock fromBlock = _waterBlocks[from.x, from.y, from.z];

        if (to.y < 0)
        {
            transferred = amount;
        }
        else if (_solidBlocks[to.x, to.y, to.z])
        {
            return amount;
        }
        else
        {
            WaterBlock toBlock = _waterBlocks[to.x, to.y, to.z];
            if (!toBlock)
            {
                GameObject newBlock = Instantiate(waterBlockPrefab, ToWorldCoors(to), Quaternion.identity);
                newBlock.transform.parent = this.transform;
                
                toBlock = newBlock.GetComponent<WaterBlock>();
                toBlock.Amount = 0;
                
                _waterBlocks[to.x, to.y, to.z] = toBlock;
                _waterBlockList.AddLast(to);
            }

            transferred = Math.Min(1 - toBlock.Amount, amount);
            toBlock.Amount += transferred;
        }
        
        fromBlock.Amount -= transferred;
        if (fromBlock.Amount == 0)
        {
            // remove block
            _waterBlocks[from.x, from.y, from.z] = null;
            _waterBlockList.Remove(from);
            Destroy(fromBlock.gameObject);
        }

        return amount - transferred;
    }

    private static Vector3Int ToWorldCoors(Vector3Int gridCoors)
    {
        gridCoors.x -= 50;
        gridCoors.z -= 50;
        return gridCoors;
    }

    private static Vector3Int ToGridCoors(Vector3Int worldCoors)
    {
        worldCoors.x += 50;
        worldCoors.z += 50;
        return worldCoors;
    }
}
