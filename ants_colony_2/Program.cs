using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ants_colony_2
{
    /* ресурсы:
     * росинка - Р
     * веточка - В
     * камушек - К
     * листик  - Л
     */
    abstract class Effect
    {
        public Random rand = new Random(DateTime.Now.Millisecond);
    }
    abstract class Insect
    {
        public int dmg;
        public double hp, effectHp, def;
        public string type;
        public Colony colony;
        public Random rand = new Random(DateTime.Now.Millisecond);

        public Insect(string type, double hp, double def, int dmg)
        {
            this.type = type;
            this.def = def;
            this.effectHp = hp;
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

        public void updHP()
        {
            this.hp = this.def + this.effectHp;
        }

        public virtual void about()
        {
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
            Console.Write($"Королева '{name}': ");
            Global.printLifeChars(this);
        }
        public void genLarvae()
        {
            larvaeNumber = rand.Next(8, 16);
        }
        public void addAntToColony(string antType)
        {
            List<Warrior> ants_Warrior = Global.getAntsExamples_Warrior((name.Length > 8) ? name.Split('_')[0] : name);
            List<Worker> ants_Worker = Global.getAntsExamples_Worker((name.Length > 8) ? name.Split('_')[0] : name);
            switch (antType)
            {
                case "Warrior":
                    colony.warriors.Add(ants_Warrior[rand.Next(ants_Warrior.Count)]);
                    break;
                case "Worker":
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
                        Colony c = Global.genColony("", qKid, 12, 9, colony.specialPrototype[0], colony.specialPrototype[0].GetType().Name);
                        c.friendColonies.Add(colony);
                        colony.friendColonies.Add(c);
                        queenKids.Add(qKid);
                        Console.WriteLine($"--Рождена королева! Основала колонию: {c.color}");
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
            Console.WriteLine($"--Выросли: р={colony.warriors.FindAll(x => (x.colony == null)).Count}, в={colony.workers.FindAll(x => (x.colony == null)).Count}");
            foreach (var warrior in colony.warriors)
            {
                warrior.colony = colony;
            }
            foreach (var worker in colony.workers)
            {
                worker.colony = colony;
            }
            genLarvae();
            Console.WriteLine($"--Новые личинки: {larvaeNumber}\n");
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
            Console.WriteLine($"--Новые личинки: {larvaeNumber}");
        }
    }
    class Worker : Insect
    {
        public string[] takeElems;
        public int countElems;
        public Worker(string type, int hp, int def, int dmg, string[] takeElems, int countElems) : base(type, hp, def, dmg)
        {
            this.takeElems = takeElems;
            this.dmg = 0;
            this.countElems = countElems;
        }

        public override void about()
        {
            Console.WriteLine($"Тип: {type}");
            Global.printLifeChars(this);
            Console.WriteLine($"--Королева '{colony.queen.name}'");
        }

        public void takeResource(List<int> resources, List<int> groupResources)
        {
            for (int i = 0; i < countElems; i++)
            {
                if (countElems == 1 && takeElems.Length>1)
                {
                    int randRes = rand.Next(2);
                    if (resources[Convert.ToInt32(takeElems[randRes])] !=0)
                    {
                        resources[Convert.ToInt32(takeElems[randRes])] -= 1;
                        groupResources[Convert.ToInt32(takeElems[randRes])] += 1;
                    }
                }
                else
                {
                    if (resources[Convert.ToInt32(takeElems[i])] !=0)
                    {
                        resources[Convert.ToInt32(takeElems[i])] -= 1;
                        groupResources[Convert.ToInt32(takeElems[i])] += 1;
                    }
                }
            }
        }

        // способность мутанта только
        public void attack(HikingGroup attackingGroup, HikingGroup enemyGroup)
        {
            antFight();
            void antFight(bool flag = false)
            {
                List<string> antsExample = enemyGroup.getAntTypes();
                string antType = antsExample[rand.Next(antsExample.Count)];
                switch (antType)
                {
                    case "Warrior":
                        List<Warrior> aliveEnemyWar = enemyGroup.warriors.FindAll(x => (x.hp != 0));
                        if (aliveEnemyWar.Count != 0)
                        {
                            Warrior ant = aliveEnemyWar[rand.Next(aliveEnemyWar.Count)];
                            while (ant.hp != 0 && hp != 0)
                            {
                                fight(ant);
                            }
                        }
                        break;
                    case "Worker":
                        List<Worker> aliveEnemyWork = enemyGroup.workers.FindAll(x => (x.hp != 0));
                        if (aliveEnemyWork.Count != 0)
                        {
                            aliveEnemyWork[rand.Next(aliveEnemyWork.Count)].subtractHP(1,1);
                        }
                        break;
                    case "Bumblebee":
                        List<Insect> aliveEnemyB = enemyGroup.special.FindAll(x => (x.GetType().Name == "Bumblebee" && x.hp != 0));
                        if (aliveEnemyB.Count != 0)
                        {
                            Bumblebee enemyB = (Bumblebee)aliveEnemyB[rand.Next(aliveEnemyB.Count)];
                            enemyB.subtractHP(1,1);
                        }
                        break;
                    case "Cricket":
                        List<Insect> aliveEnemyC = enemyGroup.special.FindAll(x => (x.GetType().Name == "Cricket" && x.hp != 0));
                        if (aliveEnemyC.Count != 0)
                        {
                            Cricket enemyC = (Cricket)aliveEnemyC[rand.Next(aliveEnemyC.Count)];
                            enemyC.subtractHP(1,1);
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
                    case "трудолюбивый обычный агрессивный аномальный сонный - Сверчок":
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
                enemyAnt.subtractHP(1, 1);
                // сейчас ударяет enemy ant
                if (enemyAnt.hp != 0)
                {
                    counterAttack(enemyAnt);
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

        public override void about()
        {
            Console.WriteLine($"Тип: {type}");
            Global.printLifeChars(this);
            Console.WriteLine($"--Королева '{colony.queen.name}'");
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
                                    if (flag) // этот код работает если атакующий муравей уже определен и производит атаку на 2 - 3 цели. флаг задается в цикле внизу
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
                    case "трудолюбивый обычный агрессивный аномальный сонный - Сверчок":
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

        public override void about()
        {
            Console.WriteLine($"Тип: {type}");
            Global.printLifeChars(this);
            Console.WriteLine($"--Королева '{colony.queen.name}'");
        }
    }
    class Cricket : Insect
    {
        public string[] canTakeResource = { "3", "2" };
        public bool canBeAttacked = true, asleep = false;
        public int targetCount, biteCount, dayOfAsleep;
        public string modifier;
        public double saved_hp;
        public Cricket(int hp, int def, int dmg, string type, bool canBeAttacked, int targetCount, int biteCount, string modifier) : base(type, hp, def, dmg)
        { 
            this.modifier = modifier;
            this.canBeAttacked = canBeAttacked;
            this.targetCount = targetCount;
            this.biteCount = biteCount;
        }

        public override void about()
        {
            Console.WriteLine($"Тип: {type}");
            Global.printLifeChars(this);
            Console.WriteLine($"--Королева '{colony.queen.name}'");
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

        public void takeResource(List<int> resources, List<int> groupResources)
        {
            for (int i = 0; i < Convert.ToInt32(canTakeResource[0]); i++)
            {
                if (resources[Convert.ToInt32(canTakeResource[1])] != 0)
                {
                    resources[Convert.ToInt32(canTakeResource[1])] -= 1;
                    groupResources[Convert.ToInt32(canTakeResource[1])] += 1;
                }
            }
        }

        public void fallAsleep(int day)
        {
            if (rand.Next(100)>70)
            {
                if (asleep == false)
                {
                    saved_hp = hp;
                    hp = 0;
                    asleep = true;
                    dayOfAsleep = day;
                }
            }
            else
            {
                if (asleep == true && (day == dayOfAsleep + 1))
                {
                    hp = saved_hp;
                    asleep = false;
                }
            }
            
        }
    }
    class Stack
    {
        public Cicada cicadaOnStack;
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
                Console.WriteLine($"-- C колонии '{group.color}' отправились: р={group.workers.Count}, в={group.warriors.Count}, о={group.special.Count} на кучу {number}");
            }
        }

        public void about(int day)
        {
            if (!isExhausted)
            {
                Console.WriteLine($"Куча { number}: " + (stackResources[0] == 0 ? "" : $"веточка: {stackResources[0]}; ") + (stackResources[1] == 0 ? "" : $"камушек: {stackResources[1]}; ") + (stackResources[2] == 0 ? "" : $"росинка: {stackResources[2]};"));
            }
            else
            {
                Console.WriteLine($"Куча {number}: истощена");
            }
            if (cicadaOnStack!= null)
            {
                cicadaOnStack.about(day);
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
                        attackingAnt.attack(attackingGroup,enemyGroup);
                    }

                    if (cicadaOnStack != null)
                    {
                        foreach (var mutantWorker in attackingGroup.workers)
                        {
                            mutantWorker.attack(attackingGroup, enemyGroup);
                        }
                    }
                }
            }
            foreach (var group in groupsOnStack)
            {
                group.calcLosses();
                group.cancelBumblebeeEffect();
            }
        }

        public void returnWarriors(string color, Colony colony)
        {
            foreach (var group in groupsOnStack)
            {
                if (group.color == color)
                {
                    foreach (var war in group.warriors.FindAll(x => (x.hp != 0)))
                    {
                        colony.warriors.Add(war);
                    }
                }
            }
        }

        public void returnWorkers(string color, Colony colony)
        {
            foreach (var group in groupsOnStack)
            {
                if (group.color == color)
                {
                    foreach (var work in group.workers.FindAll(x => (x.hp != 0)))
                    {
                        colony.workers.Add(work);
                    }
                }
            }
        }
        public void returnSpecials(string color, Colony colony)
        {
            foreach (var group in groupsOnStack)
            {
                if (group.color == color)
                {
                    foreach (var spec in group.special.FindAll(x => (x.hp != 0)))
                    {
                        colony.special.Add(spec);
                    }
                }
            }
        }

        public void antsTake(int day)
        {
            foreach (var group in groupsOnStack)
            {
                foreach (var worker in group.workers.FindAll(x => (x.hp != 0)))
                {
                    // особенность мурав старший забывчивый
                    if (worker.type == "старший забывчивый")
                    {
                        if (rand.Next(100)<70)
                        {
                            worker.takeResource(stackResources, group.groupResources);
                        }
                    }
                    else
                    {
                        worker.takeResource(stackResources, group.groupResources);
                    }
                    
                }
                foreach (var special in group.special.FindAll(x => (x.hp != 0)))
                {
                    // особенность сверчка засыпание
                    if (special.dmg>0)
                    {
                        ((Cricket)special).fallAsleep(day);
                        ((Cricket)special).takeResource(stackResources, group.groupResources);
                    }
                }
            }
            
            if (stackResources[0]+ stackResources[1] + stackResources[2] + stackResources[3]==0)
            {
                isExhausted = true;
            }
        }
        
        public void collectResources(string color, Colony colony)
        {
            foreach (var group in groupsOnStack)
            {
                if (group.color == color)
                {
                    for (int i = 0; i < colony.resources.Count; i++)
                    {
                        colony.resources[i] += group.groupResources[i];
                        colony.gapRes[i] += group.groupResources[i];
                    }
                }
            }
        }
        public void getLosses(string color,Colony colony)
        {
            foreach (var group in groupsOnStack)
            {
                if (group.color == color)
                {
                    for (int i = 0; i < group.losses.Count; i++)
                    {
                        colony.losses[i] += group.losses[i];
                    }
                }
            }
        }

        public void cleanGroups()
        {
            for (int i = 0; i < groupsOnStack.Count; i++)
            {
                groupsOnStack[i] = null;
            }
            groupsOnStack.Clear();
        }
    }
    class HikingGroup
    {
        public Colony colony;
        public string color;
        public List<Warrior> warriors;
        public List<Worker> workers;
        public List<Insect> special;
        public List<int> groupResources = new List<int>{0,0,0,0};
        public Random rand = new Random(DateTime.Now.Millisecond);
        public List<int> losses = new List<int> { 0, 0, 0 };

        public HikingGroup(Colony colony)
        {
            this.colony = colony;
            this.color = colony.color;
            this.warriors = getWarriors();
            this.workers = getWorkers();
            this.special = getSpecials();
        }
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
            int antCount = (colony.special.Count != 0) ? 1 : 0;
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
                    war.updHP();
               }
               foreach (var work in workers)
               {
                   work.def *= 2;
                    work.updHP();
                }
               foreach (var spec in special)
               {
                   spec.def *= 2;
                    spec.updHP();
                }
           }
        }

        public void cancelBumblebeeEffect()
        {
            if ((special.FindAll(x => (x.GetType().Name == "Bumblebee"))).Count != 0)
            {
                foreach (var war in warriors)
                {
                    war.def /= 2;
                    war.updHP();
                }
                foreach (var work in workers)
                {
                    work.def /= 2;
                    work.updHP();
                }
                foreach (var spec in special)
                {
                    spec.def /= 2;
                    spec.updHP();
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
            losses[0]=(workers.FindAll(x => (x.hp == 0)).Count);
            losses[1]=(warriors.FindAll(x => (x.hp == 0)).Count);
            losses[2]=(special.FindAll(x => (x.hp == 0)).Count);
        }

    }
    class Colony
    {
        public string color;
        public List<Colony> friendColonies = new List<Colony> { };
        public Queen queen;
        public List<Warrior> warriors;
        public List<string> warTypes = new List<string>();
        public List<string> workTypes = new List<string>();
        public List<string> specTypes = new List<string>();
        public List<Worker> workers;
        public List<Insect> special;
        public List<Insect> specialPrototype;
        public Cricket cricketPrint;
        public Bumblebee bumblePrint;
        public List<int> resources = new List<int>{ 0, 0, 0, 0 };
        public List<int> gapRes = new List<int> { 0, 0, 0, 0 };
        public List<int> losses = new List<int> { 0,0,0};
        public Random rand = new Random(DateTime.Now.Millisecond);

        public Colony(string color, Queen queen, int count_R, int count_W, Insect special)
        {
            this.color = color;
            this.queen = queen;
            workers = genWorkers(count_R);
            warriors = genWarriors(count_W);
            this.special = new List<Insect> { special };
            specTypes.Add(special.type);
            this.specialPrototype = new List<Insect> { special };
        }
        public List<Worker> genWorkers(int count)
        {
            List<Worker> antExamples = Global.getAntsExamples_Worker(color);
            foreach (var ant in antExamples)
            {
                workTypes.Add(ant.type);
            }
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
            foreach (var ant in antExamples)
            {
                warTypes.Add(ant.type);
            }
            List<Warrior> warriors = new List<Warrior>();
            for (int j = 0; j < count; j++)
            {
                warriors.Add(antExamples[rand.Next(2)]);
            }

            return warriors;
        }
        public void info()
        {
            Console.WriteLine($"Колония '{color}':\n--Королева: {queen.name}, личинок: {queen.larvaeNumber}");
            Console.WriteLine($"--Ресурсы: в={resources[0]}, к={resources[1]}, р={resources[2]}, л={resources[3]}");
            Console.WriteLine($"--Популяция {workers.Count + warriors.Count + special.Count}: р={workers.Count}, в={warriors.Count}, о={special.Count}\n");
        }

        public void colonyInfo()
        {
            Console.WriteLine($"Колония '{color}':");
            queen.about();
            Console.WriteLine($"--Ресурсы: в={resources[0]}, к={resources[1]}, р={resources[2]}, л={resources[3]}\n");
            if (workers.Count!= 0)
            {
                Console.WriteLine("<<<<<<<<<<<<< Рабочие >>>>>>>>>>>>>");
            }
            foreach (var type in workTypes)
            {
                var ant = workers.Find(x => x.type == type);
                if (ant != null)
                {
                    Console.WriteLine($"Тип: {type}");
                    Console.Write($"--Параметры: ");
                    Global.printLifeChars(ant);
                    Console.WriteLine($"--Количество: {workers.FindAll(x => x.type == type).Count}\n");
                }
            }
            if (warriors.Count != 0)
            {
                Console.WriteLine("<<<<<<<<<<<<< Воины >>>>>>>>>>>>>");
            }
            foreach (var type in warTypes)
            {
                var ant = warriors.Find(x => x.type == type);
                if (ant!=null)
                {
                    Console.WriteLine($"Тип: {type}");
                    Console.Write($"--Параметры: ");
                    Global.printLifeChars(ant);
                    Console.WriteLine($"--Модификатор: <{ant.modifier}>");
                    Console.WriteLine($"--Количество: {warriors.FindAll(x => x.type == type).Count}\n");
                }
            }
            if (special.Count != 0)
            {
                Console.WriteLine("<<<<<<<<<<<<< Особые >>>>>>>>>>>>>");
            }
            if (cricketPrint!=null)
            {
                Console.WriteLine($"Тип: {cricketPrint.type}");
                Console.Write($"--Параметры: ");
                Global.printLifeChars(cricketPrint);
                string[] mods = cricketPrint.modifier.Split(';');
                Console.WriteLine("--Модификаторы:");
                foreach (string mod in mods)
                {
                    Console.WriteLine($"--{mod};");
                }
                Console.WriteLine($"--Количество: {special.Count}\n");
            }
            else
            {
                Console.WriteLine($"Тип: {bumblePrint.type}");
                Console.Write($"--Параметры: ");
                Global.printLifeChars(bumblePrint);
                string[] mods = bumblePrint.modifier.Split(';');
                Console.WriteLine("--Модификаторы:");
                foreach (string mod in mods)
                {
                    Console.WriteLine($"--{mod};");
                }
                Console.WriteLine($"--Количество: {special.Count}\n");
            }
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

        public int getResourcesSum()
        {
            int sum = 0;
            foreach (var item in resources)
            {
                sum += item;
            }
            return sum;
        }
        public void antInfo()
        {
            Console.WriteLine("в разработке");
        }
        public void aboutReturn()
        {
            Console.WriteLine($"В колонию '{color}' вернулись:");
            Console.WriteLine($"--р={workers.Count}, в={warriors.Count}, о={special.Count}\n--Добыто ресурсов: в={gapRes[0]}, к={gapRes[1]}, р={gapRes[2]}, л={gapRes[3]}");
            Console.WriteLine($"--Потери: р={losses[0]} в={losses[1]} о={losses[2]}");
        }

        public void reset()
        {
            losses = new List<int> { 0, 0, 0 };
            gapRes = new List<int> { 0, 0, 0, 0 };
        }
    }
    class Cicada : Effect
    {
        public int effectDays = 7, limitDay = 2;
        public string modifier, type;
        public bool isAppear = false;
        public int dayOfAppear;

        public Cicada(string modifier, string type)
        {
            this.modifier = modifier;
            this.type = type;
        }

        public void about(int day)
        {
            Console.WriteLine($"\tГлобальный эффект: <{type}> {modifier} (в течение еще {Math.Abs(day - dayOfAppear) + 1})");
        }

        public bool willAppear(int day)
        {
            if (rand.Next(100)>60 && !isAppear)
            {
                isAppear = true;
                dayOfAppear = day;
                return true;
            }
            return false;
        }

        public bool isLimitDay(int day)
        {
            effectDays -= 1;
            if (effectDays <= 0 || (Math.Abs(day - dayOfAppear) == limitDay))
            {
                return true;
            }
            return false;
        }

        public void disappear()
        {
            isAppear = false;
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
        static public void printLifeChars(Insect ant)
        {
            Console.WriteLine($"здоровье={ant.effectHp}, защита={ant.def}, урон={ant.dmg}");
        }
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

        static public Colony genColony(string color, Queen queen, int count_R, int count_W, Insect special, string insType)
        {
            string chosenColor = colonyColors[rand.Next(colonyColors.Count)];
            colonyColors.Remove(chosenColor);
            Colony colony = new Colony((color == "") ? chosenColor : color, queen, count_R, count_W, special);
            queen.colony = colony;
            special.colony = colony;

            if (insType == "Bumblebee")
            {
                colony.bumblePrint = (Bumblebee)special;
            }
            else
            {
                colony.cricketPrint = (Cricket)special;
            }

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
    }
    class Program
    {
        static void Main(string[] args)
        {
            Random rand = new Random(DateTime.Now.Millisecond);

            // глобальный эффект
            Cicada cicada = new Cicada("появилась на куче и на ней рабочие начинают атаковать врагов", "Аномальная Певчая-цикада");
            
            // кучи
            // порядок ресурсов: В К Р Л
            Stack k1 = new Stack(1, new int[] { 28, 0, 0, 0 });
            Stack k2 = new Stack(2, new int[] { 23, 15, 0, 0 });
            Stack k3 = new Stack(3, new int[] { 43, 24, 10, 0 });
            Stack k4 = new Stack(4, new int[] { 33, 40, 0, 0 });
            Stack k5 = new Stack(5, new int[] { 41, 0, 0, 0 });
            List<Stack> stacks = new List<Stack> { k1, k2, k3, k4, k5 };

            // зеленые
            Queen queen1 = new Queen("", "Феодора", 16, 6, 25, rand.Next(2, 6), 3);
            // сделать вывод модификатара по строчкам с помощью split(";")
            Bumblebee shmel = new Bumblebee(26, 8, 0, "ленивый обычный мирный заботливый - Шмель", true, false, "не может брать ресурсы; может быть атакован войнами; зашита всех в походе увеличена в двое");
            Colony colony1 = Global.genColony("зеленые", queen1, 12, 8, shmel, shmel.GetType().Name);

            // красные
            Queen queen2 = new Queen("", "Маргрете", 15, 9, 17, rand.Next(3, 5), 4);
            // сделать вывод модификатара по строчкам с помощью split(";")
            Cricket sverhok = new Cricket(21, 5, 8, "трудолюбивый обычный агрессивный аномальный сонный - Сверчок", true, 2, 1, "может брать ресурсы (3 ресурса: росинка); может быть атакован войнами; атакует врагов(2 цели за раз и наносит 1 укус); атакует своих вместо врагов; по пути в колонию может уснуть и вернуться на следующий день");
            Colony colony2 = Global.genColony("красные", queen2, 12, 9, sverhok, sverhok.GetType().Name);

            // основной код
            Console.WriteLine(" Задайте время выдержки вывода (в млсекундах, например - 1000 или 0)");
            int userSec = Convert.ToInt32(Console.ReadLine());
            for (int day = 1; day <= Global.DRY_TIME; day++)
            {
                Console.WriteLine("\n\t\t\t\t\t#################");
                Console.WriteLine($"\t\t\t\tДень {day} (до засухи осталось {Global.DRY_TIME + 1 - day} д.)");
                screen1(day, Global.colonies);
                screen2_3Manager();
                screen4(day);
                wait(userSec);
                Console.WriteLine("\n");
            }
            // итоги
            results(Global.colonies);

            // функции 
            void results(List<Colony> colonies)
            {
                Console.WriteLine("\n\t\t\t\t\t#################");
                Console.WriteLine($"\t\t\t\t\t  Пришла засуха!");
                screen1(12, Global.colonies);
                Dictionary<string, int> maxResColony = new Dictionary<string, int>();
                foreach (var colony in colonies)
                {
                    maxResColony.Add(colony.color, colony.getResourcesSum());
                    
                }
                Console.WriteLine("\n\t\t\t\t\t##################");
                Console.WriteLine($"\t\t\tВыжила колония '{maxResColony.OrderByDescending(x => x.Value).First().Key}', набравшая суммарно {maxResColony.OrderByDescending(x => x.Value).First().Value} ресурсов");
            }

            void screen1(int day, List<Colony> colonies)
            {
                // инфо о колонии
                Console.WriteLine("\nЭкран 1 – Начало хода\n---------------------------------");
                foreach (var colony in colonies)
                {
                    colony.info();
                }

                // инфо о кучах
                foreach (var stack in stacks)
                {
                    stack.about(day);
                }
            }
            void screen2(string color)
            {
                foreach (var c in Global.colonies)
                {
                    if (c.color == color)
                        c.colonyInfo();
                }
            }
            void screen4(int day)
            {
                Console.WriteLine("\nЭкран 4 - Поход\n---------------------------------\nНачало дня:");
                goHiking(Global.colonies, day);

                Console.WriteLine("\nКонец дня:");
                returnHome(Global.colonies, day);
            }
            void larvaeGrowth(Queen queen, int day)
            {
                if (day % (queen.growthCycle + 1) == 0)
                {
                    queen.genAnt();
                }
                else
                {
                    Console.WriteLine($"--Выросли: еще растут ({Math.Abs(queen.growthCycle+1 - day)} д.)\n") ;
                }
            }

            void goHiking(List<Colony> colonies, int day)
            {
                // отправка муравьев на кучи
                foreach (var colony in colonies)
                {
                    colony.reset();
                    while (colony.warriors.Count + colony.workers.Count + colony.special.Count != 0)
                    {
                        Stack target = stacks.FindAll(x => (x.isExhausted==false))[rand.Next(stacks.FindAll(x => (x.isExhausted == false)).Count)];
                        if (target.groupsOnStack.Find(x => (x.color == colony.color)) != null)
                        {
                            HikingGroup hGroup = target.groupsOnStack.Find((x) => x.color == colony.color);
                            hGroup.supplementAnts();
                        }
                        else { target.groupsOnStack.Add(new HikingGroup(colony)); }
                    }
                }
                //на куче
                foreach (var stack in stacks)
                {
                    // глобальный эффект появление
                    if (cicada.willAppear(day))
                    {
                        stack.cicadaOnStack = cicada;
                    }
                    stack.antsFight();
                    stack.antsTake(day);
                }

                // вывод инфы:
                // С колонии «name» отправились..
                foreach (var stack in stacks)
                {
                    stack.aboutAntsOnStack();
                }
            }

            void returnHome(List<Colony> colonies, int day)
            {
                foreach (var colony in colonies.ToArray())
                {
                    foreach (var stack in stacks)
                    {
                        stack.returnWarriors(colony.color, colony);
                        stack.returnWorkers(colony.color, colony);
                        stack.collectResources(colony.color, colony);
                        stack.getLosses(colony.color, colony);
                        stack.returnSpecials(colony.color, colony);
                    }
                    colony.aboutReturn();
                    larvaeGrowth(colony.queen, day);
                }
                foreach (var stack in stacks)
                {
                    if (stack.cicadaOnStack != null)
                    {
                        if (stack.cicadaOnStack.isLimitDay(day))
                        {
                            stack.cicadaOnStack.disappear();
                            stack.cicadaOnStack = null;
                        }
                    }
                    stack.cleanGroups();
                }
            }
            void screen2_3Manager()
            {
                while (true)
                {
                    Console.WriteLine("\nЭкран 2 – Информация по колонии:\n---------------------------------");
                    Console.WriteLine("Выберите цвет колонии:");
                    foreach (var c in Global.colonies)
                    {
                        Console.WriteLine($"\t- {c.color}");
                    }
                    Console.WriteLine("\t- Для выхода введите 0");
                    string userColonyColor = Console.ReadLine();
                    if (userColonyColor == "0")
                    {
                        wait(userSec);
                        break;
                    }
                    screen2(userColonyColor);
                    wait(userSec);
                    screen3Manager(userColonyColor);
                }
            }

            void screen3Manager(string userColonyColor)
            {
                Console.WriteLine($"\nЭкран 3 – Информация по муравью в колонии '{userColonyColor}':\n---------------------------------");
                Colony chosenC = Global.colonies.Find(x => x.color == userColonyColor);
                string userAnt = "";
                while (userAnt != "0")
                {
                    Console.WriteLine("Выберите тип муравья:");
                    Console.WriteLine($"\t- 1 ~ Воин\n\t- 2 ~ Рабочий\n\t- 3 ~ Особенное\n\t- 4 ~ Королева");
                    Console.WriteLine("\t- 0 ~ Выход");
                    userAnt = Console.ReadLine();
                    switch (userAnt)
                    {
                        case "1":
                            foreach (var war in chosenC.warriors)
                            {
                                
                                war.about();
                            }
                            errorCheckPrint(chosenC.warriors.Count);
                            break;
                        case "2":
                            foreach (var work in chosenC.workers)
                            {
                                
                                work.about();
                            }
                            errorCheckPrint(chosenC.workers.Count);
                            break;
                        case "3":
                            
                            foreach (var spec in chosenC.special)
                            {
                                
                                if (chosenC.bumblePrint != null)
                                {
                                    
                                    ((Bumblebee)spec).about();
                                }
                                else
                                {
                                    ((Cricket)spec).about();
                                }
                            }
                            errorCheckPrint(chosenC.special.Count);
                            break;
                        case "4":
                            chosenC.queen.about();
                            break;
                        default:
                            Console.WriteLine("\nнеизвестное имя");
                            break;
                        case "0":
                            break;
                    }
                    Console.WriteLine();
                    wait(userSec);
                }
            }

            void errorCheckPrint(int antsCount)
            {
                if (antsCount==0)
                {
                    Console.WriteLine("\n\t..Все муравьи данного типа мертвы..");
                }
            }

            void wait(int sec)
            {
                Thread.Sleep(sec);
            }

        }
    }
}
