using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using Panda;

public class AI : MonoBehaviour
{
    //Posição do Player
    public Transform player;
    //Posição de onde as balas vão Spawnar
    public Transform bulletSpawn;
    //Slider da Barra de Vida dos Inimigos
    public Slider healthBar;   
    //Prefab da bala
    public GameObject bulletPrefab;

    //NavMeshAgent
    NavMeshAgent agent;
    //Destino aonde o inimigo irá se movimentar
    public Vector3 destination;
    //Posição do alvo
    public Vector3 target;
    //Quantia de vida
    float health = 100.0f;
    //Velocidade de rotação
    float rotSpeed = 5.0f;
    
    //Alcance em que pode ver o Player
    float visibleRange = 80.0f;
    //Alcance em que pode atirar no Player
    float shotRange = 40.0f;

    void Start()
    {
        //Pega o componente do NavMeshAgent
        agent = this.GetComponent<NavMeshAgent>();
        //Inimigo para quando o Player está a uma distância do shotRange - 5 unidades
        agent.stoppingDistance = shotRange - 5; //for a little buffer
        //Atualiza a barra de vida a cada 5 segundos
        InvokeRepeating("UpdateHealth",5,0.5f);
    }

    void Update()
    {
        //Posição da barra de Vida
        Vector3 healthBarPos = Camera.main.WorldToScreenPoint(this.transform.position);
        //Atribui o valor da vida na barra de Vida
        healthBar.value = (int)health;
        //Posiciona a barra de vida acima
        healthBar.transform.position = healthBarPos + new Vector3(0,60,0);
    }

    void UpdateHealth()
    {
        //Se vida menor que 100, incrementa
       if(health < 100)
        health ++;
    }

    [Task]
    public void PickRandomDestination()
    {
        //Pega uma posição aleatória ao redor do Player numa distância de 100 pixels
        Vector3 dest = new Vector3(Random.Range(-100, 100), 0, Random.Range(-100, 100));
        //Faz o inimigo ir até la
        agent.SetDestination(dest);
        //Termina a task e diz ao script do Panda
        Task.current.Succeed();
    }

    [Task]
    public void MoveToDestination()
    {
        if (Task.isInspected)
            Task.current.debugInfo = string.Format("t={0:0.00}", Time.time);

            if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
            {
                //Termina a task e diz ao script do Panda
                Task.current.Succeed();
            }
    }

    [Task]
    public void PickDestination(int x, int z)
    {
        //Seta o destino
        Vector3 dest = new Vector3(x, 0, z);
        //Move até o destino
        agent.SetDestination(dest);
        //Termina a task e diz ao script do Panda
        Task.current.Succeed();
    }

    [Task]
    public void TargetPlayer()
    {
        //Seta o player como o Alvo
        target = player.transform.position;
        //Termina a task e diz ao script do Panda
        Task.current.Succeed();
    }

    [Task]
    public bool Fire()
    {
        //Instancia o Prefab da bala como um GameObject na posição do bulletSpawn e com a rotação do mesmo
        GameObject bullet = GameObject.Instantiate(bulletPrefab,
            bulletSpawn.transform.position, bulletSpawn.transform.rotation);

        //Adiciona força na bala para ela ir pra frente
        bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * 2000);

        return true;
    }

    [Task]
    public void LookAtTarget()
    {
        //Cria um vetor direção do ponto A (posição do inimigo) e do Alvo (Player)
        Vector3 direction = target - this.transform.position;

        //Rotaciona o inimigo para olhar para o Alvo
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation,
            Quaternion.LookRotation(direction), Time.deltaTime * rotSpeed);

        //Se o angulo for menor do que 0.5 (o que significa que está olhando para o alvo)
        if(Vector3.Angle(this.transform.forward, direction) < 0.5f)
        {
            //Termina a task e diz ao script do Panda
            Task.current.Succeed();
        }
    }

    [Task]
    bool SeePlayer()
    {
        //Distancia do inimigo para o Player
        Vector3 distance = player.transform.position - this.transform.position;
        //Raycast
        RaycastHit hit;
        //Bool para saber se está olhando para uma parede
        bool seeWall = false;
        //Desenha o raio do Raycast na cena
        Debug.DrawRay(this.transform.position, distance, Color.red);
        //Se o raio colidir com uma parede, seta o seeWall como TRUE
        if (Physics.Raycast(this.transform.position, distance, out hit))
        {
            if (hit.collider.gameObject.tag == "wall")
            {
                seeWall = true;
            }
        }

        if (Task.isInspected)
        {
            Task.current.debugInfo = string.Format("wall={0}", seeWall);
        }

        if (distance.magnitude < visibleRange && !seeWall)
            return true;
        else
            return false;
    }

    [Task]
    bool Turn(float angle)
    {
        //Não entendi muito bem pra falar a verdade
        var p = this.transform.position + Quaternion.AngleAxis(angle, Vector3.up) * this.transform.forward;
        target = p;
        return true;
    }

    void OnCollisionEnter(Collision col)
    {
        //Se for atingido por uma bala do Player perde 10 de vida
        if(col.gameObject.tag == "bullet")
        {
            health -= 10;
        }
    }
}

