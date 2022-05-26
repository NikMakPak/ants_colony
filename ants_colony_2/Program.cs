using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ants_colony_2
{
    // названия
    /* ресурсы:
     * росинка - Р
     * веточка - В
     * камушек - К
     * листик  - Л
     */

    //TODO:
    // продолжить реазицию вывода информации экранов
    
    // доделать атаку в warrior

    // реализовать для особенных насек сбор ресурсов\
    // сделать выбор куч c учетом поля isExausted !!
    // проблему найти - мурав не берут ресурсы

    // реализовать очистку листов после битвы

    // сделать при возрвате домой зачисление ресурсов из группы в базу
    // увеличить количество личинок королевам. - большие потери
    abstract class Insect
    {
        public int dmg;
        public double hp, def;
        public string type;
        public Colony colony;
        public Random rand = new Random(DateTime.Now.Millisecond);

        public Insect(string type, double hp, double def, int dmg)
        {
            this.type = type;
            this.def = def;
            this.hp = hp + def;
            this.dmg = dmg;
        }

        public void subtractHP(int biteCount, double dmg)
        {
            hp -= biteCount * dmg;
            if (hp<0)
            {
                hp = 0;
            }
        }

        public virtual void about()
        {
            Console.WriteLine($"\n==========={GetType().Name}===========");
            Console.WriteLine($"здоровье = {hp}, защита = {def}, урон = {dmg}");
        }
    }
    class Queen : Insect
    {
        public string name;
        public int queensLimit;
        public int larvaeNumber;
        public int growthCycle;
        public List<QueenDoughter> queenKids = new List<QueenDoughter> { };
        public Queen(string type, string name, double hp, double def, int dmg, int growthCycle, int queensLimit) : base(type, hp, def, dmg)
        {
            this.type = null;
            this.name = name;
            this.queensLimit = queensLimit;
            this.growthCycle = growthCycle;
            genLarvae();
        }

        public override void about()
        {
            base.about();
            Console.WriteLine($"Имя: {name}");
        }
        public void genLarvae()
        {
            larvaeNumber = rand.Next(3, 8);
        }
        public void addAntToColony(string antType)
        {
            List<Warrior> ants_Warrior = Global.getAntsExamples_Warrior((name.Length > 8) ? name.Split('_')[0] : name);
            List<Worker> ants_Worker = Global.getAntsExamples_Worker((name.Length > 8) ? name.Split('_')[0] : name);
            switch (antType)
            {
                case "Warrior":
                    //Console.WriteLine(" воин");
                    colony.warriors.Add(ants_Warrior[rand.Next(ants_Warrior.Count)]);
                    break;
                case "Worker":
                    //Console.WriteLine(" рабочий");
                    colony.workers.Add(ants_Worker[rand.Next(ants_Worker.Count)]);
                    break;
                case "Queen":
                    QueenDoughter qKid = Global.genDoughter(colony.queen, $"{name}_дочь_{queenKids.Count + 1}");

                    if (rand.Next(1000) >= 750)
                    {
                        qKid.isLost = true;
                    }
                    else
                    {
                        Console.WriteLine("-------------------");
                        Colony c = Global.genColony("", qKid, 12, 9, colony.special[0]);
                        c.friendColonies.Add(colony);
                        colony.friendColonies.Add(c);
                        Console.WriteLine(qKid.colony);
                        c.info();
                        c.population();
                        Console.WriteLine("-------------------");
                        queenKids.Add(qKid);
                    }
                    break;
            }
        }
        public virtual void genAnt()
        {
            string[] antsExample = { "Worker", "Warrior", "Queen" };
            for (int i = 0; i < larvaeNumber; i++)
            {
                string antType = antsExample[rand.Next(3)];
                if (antType == "Queen" && queenKids.Count < queensLimit)
                {
                    addAntToColony(antType);
                }
                else
                {
                    antType = antsExample[rand.Next(2)];
                    addAntToColony(antType);
                }
            }
            foreach (var warrior in colony.warriors)
            {
                warrior.colony = colony;
            }
            foreach (var worker in colony.workers)
            {
                worker.colony = colony;
            }
            genLarvae();
        }

    }
    class QueenDoughter : Queen
    {
        public Queen mother;
        public bool isLost = false;

        public QueenDoughter(string type, string name, double hp, double def, int dmg, int growthCycle, int queensLimit, Queen mother) : base(type, name, hp, def, dmg, growthCycle, queensLimit)
        {
            growthCycle = mother.growthCycle;
            this.type = null;
            this.queensLimit = -1;
            this.mother = mother;
            genLarvae();
        }
        public override void genAnt()
        {
            string[] antsExample = { "Worker", "Warrior" };
            for (int i = 0; i < larvaeNumber; i++)
            {
                string antType = antsExample[rand.Next(2)];
                addAntToColony(antsExample[rand.Next(2)]);
            }
            foreach (var warrior in colony.warriors)
            {
                warrior.colony = colony;
            }
            foreach (var worker in colony.workers)
            {
                worker.colony = colony;
            }
            genLarvae();
        }
    }



    class Worker : Insect
    {
        public string[] takeElems;
        public int countElems;
        public List<int> takenResources = new List<int> {0,0,0,0};
        public Worker(string type, int hp, int def, int dmg, string[] takeElems, int countElems) : base(type, hp, def, dmg)
        {
            this.takeElems = takeElems;
            this.dmg = 0;
            this.countElems = countElems;
        }
        public override void about()
        {
            base.about();
            Console.WriteLine("----------take elems");
            foreach (var item in takeElems)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine($"скок взял: {countElems}");
            Console.WriteLine($"веточка: {takenResources[0]}\nкамушек: {takenResources[1]}\nросинка: {takenResources[2]}\nлистик: {takenResources[3]}");
            Console.WriteLine("Имя: в разработке..");
        }
        public void takeResource(List<int> resources)
        {
            for (int i = 0; i < countElems; i++)
            {
                if (countElems == 1 && takeElems.Length>1)
                {
                    int randRes = rand.Next(2);
                    if (resources[Convert.ToInt32(takeElems[randRes])] !=0)
                    {
                        resources[Convert.ToInt32(takeElems[randRes])] -= 1;
                        takenResources[Convert.ToInt32(takeElems[randRes])] += 1;
                    }
                }
                else
                {
                    if (resources[Convert.ToInt32(takeElems[i])] !=0)
                    {
                        resources[Convert.ToInt32(takeElems[i])] -= 1;
                        takenResources[Convert.ToInt32(takeElems[i])] += 1;
                    }
                }
            }
        }
    }
    class Warrior : Insect
    {
        public int targetCount, biteCount;
        public string modifier;

        public Warrior(string type, string modifier ,  int hp, int def, int dmg, int targetCount, int biteCount) : base(type, hp, def, dmg)
        {
            this.modifier = modifier;
            this.biteCount = biteCount;
            this.targetCount = targetCount;
        }
        public void attack(HikingGroup attackingGroup,HikingGroup enemyGroup)
        {
            antFight();
            void antFight(bool flag=false)
            {
                List<string> antsExample = enemyGroup.getAntTypes();
                string antType = antsExample[rand.Next(antsExample.Count)];
                switch (antType)
                {
                    case "Warrior":
                        List<Warrior> aliveEnemyWar = enemyGroup.warriors.FindAll(x => (x.hp != 0));
                        if (aliveEnemyWar.Count != 0)
                        {
                            switch (type)
                            {
                                case "обычный сержант":
                                    aliveEnemyWar[rand.Next(aliveEnemyWar.Count)].hp = 0;
                                    break;
                                case "обычный берсерк":
                                    Warrior enemyAnt = aliveEnemyWar[rand.Next(aliveEnemyWar.Count)];
                                    double CONST_HP = hp;
                                    while (enemyAnt.hp != 0)
                                    {
                                        if (CONST_HP != hp)
                                        {
                                            foreach (var Warrior in aliveEnemyWar)
                                            {
                                                Warrior.subtractHP(biteCount, 0.5);
                                            }
                                            hp = 0;
                                            break;
                                        }
                                        fight(enemyAnt);
                                    }
                                    break;
                                default:
                                    if (flag) // этот код работает если атакующий муравей уже определен и производит атаку на 2 - 3 цели. то есть флаг задается в цикле внизу
                                    {
                                        Warrior ant = aliveEnemyWar[rand.Next(aliveEnemyWar.Count)];
                                        while (ant.hp != 0 && hp != 0)
                                        {
                                            fight(ant);
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 1; i <= targetCount; i++)
                                        {
                                            if (hp == 0)
                                            {
                                                break;
                                            }
                                            antFight(true);
                                        }
                                    }
                                    break;
                            }
                        }
                        break;
                    case "Worker":
                        List<Worker> aliveEnemyWork = enemyGroup.workers.FindAll(x => (x.hp != 0));
                        if (aliveEnemyWork.Count != 0)
                        {
                            aliveEnemyWork[rand.Next(aliveEnemyWork.Count)].hp = 0;
                        }
                        break;
                    case "Bumblebee":
                        List<Insect> aliveEnemyB = enemyGroup.special.FindAll(x => (x.GetType().Name == "Bumblebee" && x.hp != 0));
                        if (aliveEnemyB.Count != 0)
                        {
                            Bumblebee enemyB = (Bumblebee)aliveEnemyB[rand.Next(aliveEnemyB.Count)];
                            enemyB.subtractHP(biteCount, dmg);
                        }
                        break;
                    case "Cricket":
                        List<Insect> aliveEnemyC = enemyGroup.special.FindAll(x => (x.GetType().Name == "Cricket" && x.hp != 0));
                        if (aliveEnemyC.Count != 0)
                        {
                            Cricket enemyC = (Cricket)aliveEnemyC[rand.Next(aliveEnemyC.Count)];
                            enemyC.subtractHP(biteCount, dmg);
                            if (rand.Next(10) > 5)
                            {
                                counterAttack(enemyC);
                            }
                            else
                            {
                                enemyC.attackOurs(enemyGroup);
                            }
                        }
                        break;
                }
            }

            void counterAttack(Insect counterAttackingAnt)
            {
                switch (counterAttackingAnt.type)
                {
                    case "обычный сержант":
                        subtractHP(((Warrior)counterAttackingAnt).biteCount, ((Warrior)counterAttackingAnt).dmg);
                        break;
                    case "обычный берсерк":
                        foreach (var Warrior in attackingGroup.warriors.FindAll(x => (x.hp != 0)))
                        {
                            Warrior.subtractHP(((Warrior)counterAttackingAnt).biteCount, ((Warrior)counterAttackingAnt).dmg / 2);
                        }
                        counterAttackingAnt.hp = 0;
                        break;
                    case "трудолюбивый обычный агрессивный аномальный сонный":
                        subtractHP(((Cricket)counterAttackingAnt).biteCount, ((Cricket)counterAttackingAnt).dmg);
                        break;
                    default:
                        subtractHP(((Warrior)counterAttackingAnt).biteCount, ((Warrior)counterAttackingAnt).dmg);
                        break;
                }
            }
            void fight(Warrior enemyAnt)
            {
                // сейчас ударяет this ant
                enemyAnt.subtractHP(biteCount, dmg);
                // сейчас ударяет enemy ant
                if (enemyAnt.hp != 0)
                {
                    counterAttack(enemyAnt);
                }
            }
        }
        
    }
    class Bumblebee : Insect
    {
        public bool canBeAttacked = true;
        public bool takeResource = false;
        public int goDefenseSkill = 2;
        public string modifier;

        public Bumblebee(int hp, int def, int dmg, string type, bool canBeAttacked, bool takeResource, string modifier) : base(type, hp, def, dmg)
        {
            this.modifier = modifier;
            this.canBeAttacked = canBeAttacked;
            this.takeResource = takeResource;
        }

        
    }

    class Cricket : Insect
    {
        public string[] takeResource = { "3", "Р" };
        public bool canBeAttacked = true;
        public int targetCount, biteCount;
        public string modifier;

        public Cricket(int hp, int def, int dmg, string type, bool canBeAttacked, int targetCount, int biteCount, string modifier) : base(type, hp, def, dmg)
        {
            this.modifier = modifier;
            this.canBeAttacked = canBeAttacked;
            this.targetCount = targetCount;
            this.biteCount = biteCount;
        }

        public void attackOurs(HikingGroup enemyGroup)
        {
            List<Warrior> eWar = enemyGroup.warriors.FindAll(x => (x.hp != 0));
            List<Worker> eWork = enemyGroup.workers.FindAll(x => (x.hp != 0));
            List<Insect> eAll = new List<Insect> { };
            eAll.AddRange(eWar);
            eAll.AddRange(eWork);
            if (eAll.Count!=0)
            {
                for (int i = 0; i < targetCount; i++)
                {
                    Insect ant = eAll[rand.Next(eAll.Count)];
                    ant.subtractHP(biteCount, dmg);
                }
            }
        }

    }

    class Stack
    {
        public int number;
        public List<int> stackResources;
        public List<HikingGroup> groupsOnStack = new List<HikingGroup> { };
        public Random rand = new Random(DateTime.Now.Millisecond);
        public bool isExhausted = false;


        public Stack(int number, int[] stackResources)
        {
            this.number = number;
            this.stackResources = new List<int>(stackResources);
        }

        public void aboutAntsOnStack()
        {
            foreach (var group in groupsOnStack)
            {
                Console.WriteLine($"- C колонии '{group.color}' отправились: р={group.workers.Count}, в={group.warriors.Count}, о={group.special.Count} на кучу {number}");
            }
        }

        public void about()
        {
            if (stackResources.Sum() > 0)
            {
                Console.WriteLine($"Куча { number}: " + (stackResources[0] == 0 ? "" : $"веточка: {stackResources[0]}; ") + (stackResources[1] == 0 ? "" : $"камушек: {stackResources[1]}; ") + (stackResources[2] == 0 ? "" : $"росинка: {stackResources[2]};"));
            }
            else
            {
                Console.WriteLine($"Куча {number}: истощена");
            }
        }
        public HikingGroup getEnemyGroup(HikingGroup attacking, List<HikingGroup> otherGroups)
        {
            List < HikingGroup > enemies = new List < HikingGroup> { };
            for (int i = 0; i < otherGroups.Count; i++)
            {
                // проверка на союзы
                if (!attacking.colony.isFriend(otherGroups[i]))
                {
                    enemies.Add(otherGroups[i]);
                }
            }
            return (enemies.Count!=0) ? enemies[rand.Next(enemies.Count)] : null;
        }
        public void antsFight()
        {
            foreach (var group in groupsOnStack)
            {
                group.aplyBumblebeeEffect();
            }
            // определение атакующего и поиск цели
            for (int i = 0; i < groupsOnStack.Count; i++)
            {
                HikingGroup attackingGroup = groupsOnStack[i];
                HikingGroup enemyGroup = getEnemyGroup(attackingGroup, groupsOnStack.FindAll(x => (x.color != attackingGroup.color)));
                if (enemyGroup != null)
                {
                    foreach (var attackingAnt in attackingGroup.warriors)
                    {
                        // сделать  убийство через зануление хп, а выживших добавлять в новый массив с помощью фильтра по хп!=0 полсе всех драк
                        attackingAnt.attack(attackingGroup,enemyGroup);
                    }
                    // сделать чистку всех груп от нулевых муравьев перед новым заходом
                }
            }
            foreach (var group in groupsOnStack)
            {
                group.calcLosses();
            }
        }

        public List<Warrior> returnWarriors(string color)
        {
            foreach (var group in groupsOnStack)
            {
                if (group.color == color)
                {
                    return group.warriors.FindAll(x => (x.hp != 0));
                }
            }
            return null;
        }

        public List<Worker> returnWorkers(string color)
        {
            foreach (var group in groupsOnStack)
            {
                if (group.color == color)
                {
                    return group.workers.FindAll(x => (x.hp != 0));
                }
            }
            return null;
        }
        public List<Insect> returnSpecials(string color)
        {
            foreach (var group in groupsOnStack)
            {
                if (group.color == color)
                {
                    return group.special.FindAll(x => (x.hp != 0));
                }
            }
            return null;
        }

        public void antsTake()
        {
            foreach (var group in groupsOnStack)
            {
                foreach (var worker in group.workers)
                {
                    worker.takeResource(stackResources);
                }
            }
            if (stackResources[0]+ stackResources[1] + stackResources[2] + stackResources[3]==0)
            {
                isExhausted = true;
            }
        }
    }
    class HikingGroup
    {
        public Colony colony;
        public string color;
        public List<Warrior> warriors;
        public List<Worker> workers;
        public List<Insect> special;
        public List<int> resources = new List<int>{0,0,0,0 };
        public Random rand = new Random(DateTime.Now.Millisecond);
        public List<int> losses = new List<int>();
        // Р В О

        public HikingGroup(Colony colony)
        {
            this.colony = colony;
            this.color = colony.color;
            this.warriors = getWarriors();
            this.workers = getWorkers();
            this.special = getSpecials();
            //Console.WriteLine($"было зачислено {special.Count} . В колонии осталось {colony.special.Count}");
        }
        // нужно изменить распределение муравьев! сейчас добавляется огромное количество мини походов
        public List<Warrior> getWarriors()
        {
            int antCount = (colony.warriors.Count != 0) ? rand.Next(colony.warriors.Count) + 1 : rand.Next(colony.warriors.Count);
            List<Warrior> export = colony.warriors.GetRange(0, antCount);
            colony.warriors.RemoveRange(0, antCount);
            return export;
        }
        public List<Worker> getWorkers()
        {
            int antCount = (colony.workers.Count != 0) ? rand.Next(colony.workers.Count) + 1 : rand.Next(colony.workers.Count);
            List<Worker> export = colony.workers.GetRange(0, antCount);
            colony.workers.RemoveRange(0, antCount);
            return export;
        }
        public List<Insect> getSpecials()
        {
            int antCount = (colony.special.Count != 0) ? rand.Next(colony.special.Count) + 1 : rand.Next(colony.special.Count);
            List<Insect> export = colony.special.GetRange(0, antCount);
            colony.special.RemoveRange(0, antCount);
            return export;
        }
        public void supplementAnts()
        {
            foreach (var ant in getWarriors())
            {
                warriors.Add(ant);
            }
            foreach (var ant in getWorkers())
            {
                workers.Add(ant);
            }
            foreach (var ant in getSpecials())
            {
                special.Add(ant);
            }
        }

        public void aplyBumblebeeEffect()
        {
           if ((special.FindAll(x => (x.GetType().Name == "Bumblebee"))).Count != 0)
           {
               foreach (var war in warriors)
               {
                   war.def *= 2;
               }
               foreach (var work in workers)
               {
                   work.def *= 2;
               }
               foreach (var spec in special)
               {
                   spec.def *= 2;
               }
           }
        }

        public void cancelBumblebeeEffect(List<HikingGroup> groups)
        {
            if ((special.FindAll(x => (x.GetType().Name == "Bumblebee"))).Count != 0)
            {
                foreach (var war in warriors)
                {
                    war.def /= 2;
                }
                foreach (var work in workers)
                {
                    work.def /= 2;
                }
                foreach (var spec in special)
                {
                    spec.def /= 2;
                }
            }
        }

        public List<string> getAntTypes()
        {
            List<string> antsExample = new List<string> { };
            if (warriors.Count != 0)
            {
                antsExample.Add("Warrior");
            }
            if (workers.Count != 0)
            {
                antsExample.Add("Worker");
            }
            if (special.FindAll(x => (x.GetType().Name == "Bumblebee")).Count != 0)
            {
                antsExample.Add("Bumblebee");
            }
            if (special.FindAll(x => (x.GetType().Name == "Cricket")).Count != 0)
            {
                antsExample.Add("Cricket");
            }
            return antsExample;
        }

        public void calcLosses()
        {
            // Р В О
            losses.Add(workers.FindAll(x => (x.hp == 0)).Count);
            losses.Add(warriors.FindAll(x => (x.hp == 0)).Count);
            losses.Add(special.FindAll(x => (x.hp == 0)).Count);
        }

    }
    class Colony
    {
        public string color;
        public List<Colony> friendColonies = new List<Colony> { };
        public Queen queen;
        public List<Warrior> warriors;
        public List<Worker> workers;
        public List<Insect> special;
        public List<int> resources = new List<int>{ 0, 0, 0, 0 };
        public Random rand = new Random(DateTime.Now.Millisecond);

        public Colony(string color, Queen queen, int count_R, int count_W, Insect special)
        {
            this.color = color;
            this.queen = queen;
            workers = genWorkers(count_R);
            warriors = genWarriors(count_W);
            this.special = new List<Insect> { special };
        }
        public List<Worker> genWorkers(int count)
        {
            List<Worker> antExamples = Global.getAntsExamples_Worker(color);
            List<Worker> workers = new List<Worker>();
            for (int j = 0; j < count; j++)
            {
                workers.Add(antExamples[rand.Next(2)]);
            }
            return workers;

        }
        public List<Warrior> genWarriors(int count)
        {
            List<Warrior> antExamples = Global.getAntsExamples_Warrior(color);
            List<Warrior> warriors = new List<Warrior>();
            for (int j = 0; j < count; j++)
            {
                warriors.Add(antExamples[rand.Next(2)]);
            }

            return warriors;
        }
        public void info()
        {
            Console.WriteLine($"Колония {color}:\n--Королева: {queen.name}, личинок: {queen.larvaeNumber}");
            Console.WriteLine($"Колония {color} дружит с {friendColonies[0].color}");
            Console.WriteLine($"--Ресурсы: в={resources[0]} к={resources[1]} р={resources[2]} л={resources[3]}");
        }
        public bool isFriend(HikingGroup target)
        {
            foreach (var colony in friendColonies)
            {
                if (colony.color == target.color)
                {
                    return true;
                }
            }
            return false;
        }
        public void population()
        {
            Console.WriteLine($"--Популяция {workers.Count + warriors.Count + special.Count}: р={workers.Count} в={warriors.Count} о={special.Count}\n");
        }
        //public int getResourcesSum()
        //{
        //    int sum = 0;
        //    foreach (var item in resources)
        //    {
        //        sum += item.Value;
        //    }
        //    return sum;
        //}
        public void antInfo()
        {

            Console.WriteLine("в разработке");
        }

    }
    class Global
    {
        // порядок ресурсов: В К Р Л
        //                   0 1 2 3
        static public List<String> colonyColors = new List<string> { "синие", "белые", "желтые", "фиолетовые", "оранжевые", "голубые", "черные", "салатовые", "пурпурные", "коричневые", "золотые" };
        static public int DRY_TIME = 12;
        static public List<Colony> colonies = new List<Colony> { };
        static public Random rand = new Random(DateTime.Now.Millisecond);
        static public List<Worker> getAntsExamples_Worker(string qName)
        {
            if (qName == "Феодора")
            {
                return new List<Worker>()
                {
                    new Worker("продвинутый",6,2,0,new string[] {"0","3"},2),
                    new Worker("старший капризный",2,1,0,new string[] {"3","1"},1),
                };
            }
            return new List<Worker>()
            {
                new Worker("обычный",1,0,0,new string[] {"0","2"},1),
                new Worker("старший забывчивый",2,1,0,new string[] {"1","2"},1),
            };
        }

        

        static public List<Warrior> getAntsExamples_Warrior(string qName)
        {
            if (qName == "Феодора")
            {
                return new List<Warrior>()
                {
                    new Warrior("элитный", "может атаковать 2 цели за раз и наносит 2 укуса",8, 4, 3, 2,2),
                    new Warrior("обычный сержант", "может атаковать 1 цель за раз и наносит 1 укус; если атакует первый в походе, то убивает с одного укуса любое насекомое даже неуязвимое",1, 0, 1, 1,1)
                };
            }
            return new List<Warrior>()
            {
                new Warrior("легендарный","может атаковать 3 цели за раз и наносит 1 укус", 10, 6, 6, 3,1),
                new Warrior("элитный","может атаковать 2 цели за раз и наносит 2 укуса", 8, 4, 3, 2,2),
                new Warrior("обычный берсерк","может атаковать 1 цель за раз и наносит 1 укус; если получил урон, то наносит половину урона всем войнам врага и погибает", 1, 0, 1, 1,1)
            };
        }

        static public QueenDoughter genDoughter(Queen mother, string name)
        {
            return new QueenDoughter("", name, mother.hp, mother.def, mother.dmg, mother.growthCycle, mother.queensLimit, mother);
        }

        static public Colony genColony(string color, Queen queen, int count_R, int count_W, Insect special)
        {
            string chosenColor = colonyColors[rand.Next(colonyColors.Count)];
            colonyColors.Remove(chosenColor);
            Colony colony = new Colony((color == "") ? chosenColor : color, queen, count_R, count_W, special);
            queen.colony = colony;
            special.colony = colony;
            foreach (var warrior in colony.warriors)
            {
                warrior.colony = colony;
            }
            foreach (var worker in colony.workers)
            {
                worker.colony = colony;
            }
            colonies.Add(colony);
            return colony;
        }
        //    Random rand = new Random();
        //    return rand.Next(start, end + 1);

        //static public int getRndNumber(int start, int end)
        //{
        //    //Console.WriteLine(rand.Next(start, end + 1));
        //}
        //public static Insect newAnt()
        //{
        //    Global.Rand(0, 3);
        //    return new Worker();

        //}

    }

    class Program
    {


        static void Main(string[] args)
        {
            Random rand = new Random(DateTime.Now.Millisecond);
            // кучи
            // порядок ресурсов: В К Р Л
            Stack k1 = new Stack(1, new int[] { 28, 0, 0, 0 });
            Stack k2 = new Stack(2, new int[] { 23, 15, 0, 0 });
            Stack k3 = new Stack(3, new int[] { 43, 24, 10, 0 });
            Stack k4 = new Stack(4, new int[] { 33, 40, 0, 0 });
            Stack k5 = new Stack(5, new int[] { 41, 0, 0, 0 });
            Stack[] stacks = new Stack[] { k1, k2, k3, k4, k5 };

            // зеленые
            Queen queen1 = new Queen("", "Феодора", 16, 6, 25, rand.Next(2, 6), 3);
            // сделать вывод модификатара по строчкам с помощью split(";")
            Bumblebee shmel = new Bumblebee(26, 8, 0, "ленивый обычный мирный заботливый", true, false, "не может брать ресурсы; может быть атакован войнами; зашита всех в походе увеличена в двое");
            Colony colony1 = Global.genColony("зеленые", queen1, 12, 8, shmel);

            // красные
            Queen queen2 = new Queen("", "Маргрете", 15, 9, 17, rand.Next(3, 5), 4);
            // сделать вывод модификатара по строчкам с помощью split(";")
            Cricket sverhok = new Cricket(21, 5, 8, "трудолюбивый обычный агрессивный аномальный сонный", true, 2, 1, "может брать ресурсы (3 ресурса: росинка); может быть атакован войнами; атакует врагов(2 цели за раз и наносит 1 укус); атакует своих вместо врагов; по пути в колонию может уснуть и вернуться на следующий день");
            Colony colony2 = Global.genColony("красные", queen2, 12, 9, sverhok);

            // функции 
            void screen1(int day)
            {
                Console.WriteLine("\nЭкран 1 – Начало хода\n---------------------------------");
                Console.WriteLine($"День: {day} (до засухи осталось {Global.DRY_TIME - day} дней)");
                colony1.info();
                colony1.population();
                colony2.info();
                colony2.population();
                foreach (var item in stacks)
                {
                    item.about();
                }
                // глобальный эфект
            }
            void screen2()
            {
                Console.WriteLine("\nЭкран 2 – Информация по колонии\n---------------------------------");
                colony1.antInfo();

            }
            void screen3(int day)
            {
                Console.WriteLine("\nЭкран 3 - Поход\n---------------------------------\nНачало дня:");
                goHiking(Global.colonies);

                Console.WriteLine("\nКонец дня:");
                returnHome(Global.colonies, day);
                Console.WriteLine("---------------------------------");

            }
            void larvaeGrowth(Queen queen, int day)
            {
                if (day % (queen.growthCycle + 1) == 0)
                {
                    Console.WriteLine(queen.name);
                    queen.genAnt();
                }
            }

            void goHiking(List<Colony> colonies)
            {
                // отправка муравьев на кучи
                foreach (var colony in colonies)
                {
                    while (colony.warriors.Count + colony.workers.Count + colony.special.Count != 0)
                    {
                        Stack target = stacks[rand.Next(stacks.Length)];
                        if (target.groupsOnStack.Find((x) => x.color == colony.color) != null)
                        {
                            HikingGroup hGroup = target.groupsOnStack.Find((x) => x.color == colony.color);
                            hGroup.supplementAnts();
                        }
                        else { target.groupsOnStack.Add(new HikingGroup(colony)); }
                    }
                }
                // на куче
                //foreach (var stack in stacks)
                //{
                //    // набросок функций
                //    Console.WriteLine($"\nдействия на куче: {stack.number}");
                //    stack.antsFight();
                //    stack.antsTake();
                //}
                //// вывод просто дебаг инфы
                //foreach (var stack in stacks)
                //{
                //    Console.WriteLine();
                //    stack.aboutAntsOnStack();
                //}
            }

            void returnHome(List<Colony> colonies, int day)
            {
                foreach (var colony in colonies)
                {
                    foreach (var stack in stacks)
                    {
                        colony.warriors = (stack.returnWarriors(colony.color) != null) ? stack.returnWarriors(colony.color) : new List<Warrior> { };
                        colony.workers = (stack.returnWorkers(colony.color) != null) ? stack.returnWorkers(colony.color) : new List<Worker> { };
                        //foreach (var worker in colony.workers)
                        //{
                        //    worker.putResource();
                        //}
                        colony.special = (stack.returnSpecials(colony.color) != null) ? stack.returnSpecials(colony.color) : new List<Insect> { };
                        //colony.special.putResource();
                        if (colony.warriors.Count + colony.workers.Count + colony.special.Count != 0)
                        {
                            break;
                        }
                    }
                    // если идут зеленые и красные то у них нет союзников. как быть? ошибка
                    larvaeGrowth(colony.queen, day);
                }
                // выросли личинки
                
            }

            // основной код
            for (int day = 1; day <= Global.DRY_TIME; day++)
            {
                Console.WriteLine($"\nдень - {day}");
                //foreach (var colony in Global.colonies.GetRange(0, 2))
                //{
                //    larvaeGrowth(colony.queen, day);
                //}
                Console.WriteLine("\n\n");
                //screen1(day);
                //screen2();
            }
            screen3(3);
            //Console.WriteLine("++++++до");
            //k3.about();
            //colony1.workers[12].takeResource(k3.stackResources);
            //colony1.workers[12].about();
            //k3.about();
            //List<Insect> insects = new List<Insect> { colony1.warriors[0] };
            //insects[0].hp -= 1;
            //Warrior w = (Warrior)insects[0];
            //w.about();
            // результаты
            foreach (var colony in Global.colonies)
            {
                Console.WriteLine(colony.queen.name);
                Console.WriteLine(colony.color);
                colony.population();
            }
            Console.WriteLine(" пришла засуха! ");
            Console.WriteLine("++++++");
            /*
            //q1.colony = colony1;
            //q1.genAnt();
            //colony1.Info();
            Stack st1 = new Stack(1, new int[] { 0, 0, 1 });
            st1.About();
            q1.About();
            Console.WriteLine(Rand(q1.growthCycle[0], q1.growthCycle[1]));
            colony.population["рабочих"] += 1;
            Console.WriteLine(colony.population["рабочих"]); 
            */

        }
    }
}
