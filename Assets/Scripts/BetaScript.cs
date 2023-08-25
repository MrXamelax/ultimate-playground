using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using UnityEngine.Jobs;

public class BetaScript : MonoBehaviour {

    [SerializeField] private bool useJobs;
    [SerializeField] private Transform pfCat;
    private List<Cat> catList;

    // Katzen Klasse, basic
    public class Cat {
        public Transform transform;
        public float moveY;
    }

    private void Start() {

        // Katzen anlegen und spawnen und bla, eigentlich basic shit
        catList = new List<Cat>();
        for (int i = 0; i < 1000; i++) {
            Transform catTransform = Instantiate(pfCat, new Vector3(UnityEngine.Random.Range(-8f, 8f), UnityEngine.Random.Range(-5f, 5f)), Quaternion.identity);
            catList.Add(new Cat {
                transform = catTransform,
                moveY = UnityEngine.Random.Range(1f, 2f)
            });
        }
    }

    void Update() {

        if (useJobs) { // Schalter um Stats von mit/ohne Jobs zu sehen

            // Arrays mit Werten zum übergeben für ParallelJob
            NativeArray<float3> positionArray = new NativeArray<float3>(catList.Count, Allocator.TempJob);
            NativeArray<float> moveYArray = new NativeArray<float>(catList.Count, Allocator.TempJob);
            //TransformAccessArray transformAccessArray = new TransformAccessArray(catList.Count);


            for (int i = 0; i < catList.Count; i++) {
                positionArray[i] = catList[i].transform.position;
                moveYArray[i] = catList[i].moveY;
                //transformAccessArray.Add(catList[i].transform);
            }

            // Job anlegen und "bestücken"
            MyFirstJobbParallel myFirstJobbParallel = new MyFirstJobbParallel {
                deltaTime = Time.deltaTime,
                positionArray = positionArray,
                moveYArray = moveYArray,
            };

            // Job(s) schedulen
            JobHandle jobHandle = myFirstJobbParallel.Schedule(catList.Count, 100);

            // Job(s) ausführen
            jobHandle.Complete();
            
            /*
            AnotherJob anotherJob = new AnotherJob {
                deltaTime = Time.deltaTime,
                moveYArray = moveYArray,
            };

            JobHandle jobHandle = anotherJob.Schedule(transformAccessArray);
            jobHandle.Complete();
            */

            // Originale Daten anpassen/überschreiben, da mit Kopien gearbeitet wurde
            for (int i = 0; i < catList.Count; i++) {
                catList[i].transform.position = positionArray[i];
                catList[i].moveY = moveYArray[i];
            }

            // Native Collections immer disposen, sonst gibt's böse Memory Leaks!
            positionArray.Dispose();
            moveYArray.Dispose();
            //transformAccessArray.Dispose();

        } else {
            foreach (Cat cat in catList) {

                // Katze bewegt sich in y-Richtung
                cat.transform.position += new Vector3(0, cat.moveY * Time.deltaTime);

                // Katze ändert y-Richtung wenn oben oder unten angekommen
                if (cat.transform.position.y > 5f) {
                    cat.moveY = -math.abs(cat.moveY);
                }
                if (cat.transform.position.y < -5f) {
                    cat.moveY = +math.abs(cat.moveY);
                }

                // Katze macht krasses CPU-intense Pathfinding (wir tun zumindest so)
                float value = 0f;
                for (int j = 0; j < 1000; j++) {
                    value = math.exp10(math.sqrt(value));
                }
            }
        }

        /*
        if (useJobs) {
            NativeList<JobHandle> jobHandleList = new NativeList<JobHandle>(Allocator.Temp);
            for (int i = 0; i < 10; i++) {
                JobHandle jobHandle = DoSomeJob();
                jobHandleList.Add(jobHandle);
            }
            JobHandle.CompleteAll(jobHandleList);
            jobHandleList.Dispose();
        } else {
            for (int i = 0; i < 10; i++) {
                float value = 0f;
                for (int j = 0; j < 500000; j++) {
                    value = math.exp10(math.sqrt(value));
                }
            }
        }
        */

    }

    // Job scheduele einfach nur in Methode ausgelagert (nicht unbedingt nötig)
    private JobHandle DoSomeJob() {
        MyFirstJobb job = new MyFirstJobb();
        return job.Schedule();
    }

}

[BurstCompile] // <-- this is very wichtig für noch mehr performance
public struct MyFirstJobb : IJob { // Job mit Informationen und Verhalten definieren

    // Verhalten
    public void Execute() {
        float value = 0f;
        for (int i = 0; i < 500000; i++) {
            value = math.exp10(math.sqrt(value));
        }
    }
}

[BurstCompile] // <-- this is very wichtig für noch mehr performance
public struct MyFirstJobbParallel : IJobParallelFor { // Job mit Informationen und Verhalten definieren

    // Informationen
    public NativeArray<float3> positionArray;
    public NativeArray<float> moveYArray;
    public float deltaTime;

    // Verhalten
    public void Execute(int index) {

        // Katze bewegt sich in y-Richtung
        positionArray[index] += new float3(0, moveYArray[index] * deltaTime, 0);

        // Katze ändert y-Richtung wenn oben oder unten angekommen
        if (positionArray[index].y > 5f) {
            moveYArray[index] = -math.abs(moveYArray[index]);
        }
        if (positionArray[index].y < -5f) {
            moveYArray[index] = +math.abs(moveYArray[index]);
        }


        float value = 0f;
        for (int j = 0; j < 1000; j++) {
            value = math.exp10(math.sqrt(value));
        }
    }
}

[BurstCompile] // <-- this is very wichtig für noch mehr performance
public struct AnotherJob : IJobParallelForTransform { // Job mit Informationen und Verhalten definieren

    // Informationen
    public NativeArray<float> moveYArray;
    public float deltaTime;

    // Verhalten
    public void Execute(int index, TransformAccess transform) {

        // Katze bewegt sich in y-Richtung
        transform.position += new Vector3(0, moveYArray[index] * deltaTime, 0);

        // Katze ändert y-Richtung wenn oben oder unten angekommen
        if (transform.position.y > 5f) {
            moveYArray[index] = -math.abs(moveYArray[index]);
        }
        if (transform.position.y < -5f) {
            moveYArray[index] = +math.abs(moveYArray[index]);
        }

        // Katze macht krasses CPU-intense Pathfinding (wir tun zumindest so)
        float value = 0f;
        for (int i = 0; i < 1000; i++) {
            value = math.exp10(math.sqrt(value));
        }
    }
}
