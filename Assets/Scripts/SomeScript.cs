using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;
using Unity.Burst;

public class SomeScript : MonoBehaviour {

    // Schalter für Job System
    [SerializeField] private bool useJobs;

    // Prefab für die Katze
    [SerializeField] private Transform pfCat;
    private List<Cat> catList;

    // Katzenobjekt mit Attributen
    public class Cat {
        public Transform transform;
        public float moveY;
    }

    private void Start() {
        // Katzen anlegen und spawnen
        catList = new List<Cat>();
        for (int i = 0; i < 1000; i++) {

            Transform catTransform = Instantiate(pfCat, new Vector3(UnityEngine.Random.Range(-8f, 8f), UnityEngine.Random.Range(-5f, 5f)), Quaternion.identity);
            
            catList.Add(new Cat {
                transform = catTransform,
                moveY = UnityEngine.Random.Range(1f, 2f) // Geschwindigkeit soll bisschen random sein
            });
        }
        
    }

    void Update() {
        float startTime = Time.realtimeSinceStartup;

        if (useJobs) {
            // Katzen bewegen mit Job System

            NativeArray<float3> positionArray = new NativeArray<float3>(catList.Count, Allocator.TempJob);
            NativeArray<float> moveYArray = new NativeArray<float>(catList.Count, Allocator.TempJob);
            //TransformAccessArray transformAccessArray = new TransformAccessArray(catList.Count);

            for (int i = 0; i < catList.Count; i++) {
                positionArray[i] = catList[i].transform.position;
                moveYArray[i] = catList[i].moveY;
                //transformAccessArray.Add(catList[i].transform);
            }

            MyFirstJobParallel myFirstJobParallel = new MyFirstJobParallel {
                deltaTime = Time.deltaTime,
                positionArray = positionArray,
                moveYArray = moveYArray,
            };

            JobHandle jobHandle = myFirstJobParallel.Schedule(catList.Count, 100);
            jobHandle.Complete();

            for (int i = 0; i < catList.Count; i++) {
                catList[i].transform.position = positionArray[i];
                catList[i].moveY = moveYArray[i];
            }

            /*
            MyFirstJobParallelTransform myFirstJobParallelTransform = new MyFirstJobParallelTransform {
                deltaTime = Time.deltaTime,
                moveYArray = moveYArray,
            };

            jobHandle = myFirstJobParallelTransform.Schedule(transformAccessArray);
            jobHandle.Complete();
            */

            positionArray.Dispose();
            moveYArray.Dispose();
            //transformAccessArray.Dispose();

        } else {
            // Katzen bewegen wie gewohnt
            foreach (Cat cat in catList) {
                cat.transform.position += new Vector3(0, cat.moveY * Time.deltaTime);
                if (cat.transform.position.y > 5f) {
                    cat.moveY = -math.abs(cat.moveY);
                }
                if (cat.transform.position.y < -5f) {
                    cat.moveY = +math.abs(cat.moveY);
                }
                float value = 0f;
                for (int i = 0; i < 1000; i++) {
                    value = math.exp10(math.sqrt(value));
                }
            }
        }

        /*
        if (useJobs) {
            NativeList<JobHandle> jobHandleList = new NativeList<JobHandle>(Allocator.Temp);
            for (int i = 0; i < 10; i++) {
                JobHandle jobHandle = DoSomethingJob();
                jobHandleList.Add(jobHandle);
            }
            JobHandle.CompleteAll(jobHandleList);
            jobHandleList.Dispose();
        } else {
            for (int i = 0; i < 10; i++) {
                DoSomething();
            }
        }
        */

        Debug.Log(((Time.realtimeSinceStartup - startTime) * 1000f) + " ms");
    }

    // Rechenaufwendiges Zeug
    void DoSomething() {
        float value = 0f;
        for (int i = 0; i < 50000; i++) {
            value = math.exp10(math.sqrt(value));
        }
    }

    // Job ausführen und Handle returnen
    private JobHandle DoSomethingJob() {
        MyFirstJob job = new MyFirstJob();
        return job.Schedule();
    }

}

[BurstCompile]
public struct MyFirstJob : IJob {

    public void Execute() {
        float value = 0f;
        for (int i = 0; i < 50000; i++) {
            value = math.exp10(math.sqrt(value));
        }
    }
}

[BurstCompile]
public struct MyFirstJobParallel : IJobParallelFor {

    public NativeArray<float3> positionArray;
    public NativeArray<float> moveYArray;
    public float deltaTime;

    public void Execute(int index) {
        positionArray[index] += new float3(0, moveYArray[index] * deltaTime, 0);
        if (positionArray[index].y > 5f) {
            moveYArray[index] = -math.abs(moveYArray[index]);
        }
        if (positionArray[index].y < -5f) {
            moveYArray[index] = +math.abs(moveYArray[index]);
        }
        float value = 0f;
        for (int i = 0; i < 1000; i++) {
            value = math.exp10(math.sqrt(value));
        }
    }
}

[BurstCompile]
public struct MyFirstJobParallelTransform : IJobParallelForTransform {

    public NativeArray<float> moveYArray;
    public float deltaTime;

    public void Execute(int index, TransformAccess transform) {
        transform.position += new Vector3(0, moveYArray[index] * deltaTime, 0);
        if (transform.position.y > 5f) {
            moveYArray[index] = -math.abs(moveYArray[index]);
        }
        if (transform.position.y < -5f) {
            moveYArray[index] = +math.abs(moveYArray[index]);
        }

        float value = 0f;
        for (int i = 0; i < 1000; i++) {
            value = math.exp10(math.sqrt(value));
        }
    }
}
