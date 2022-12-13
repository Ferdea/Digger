using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Digger
{
    public class CreatureMapCreator
    {
        private readonly Dictionary<string, Func<ICreature>> factory = new Dictionary<string, Func<ICreature>>();

        public ICreature[,] CreateMap(string map, Game game, string separator = "\r\n")
        {
            var rows = map.Split(new[] {separator}, StringSplitOptions.RemoveEmptyEntries);
            if (rows.Select(z => z.Length).Distinct().Count() != 1)
                throw new Exception($"Wrong test map '{map}'");
            var result = new ICreature[rows[0].Length, rows.Length];
            for (var x = 0; x < rows[0].Length; x++)
            for (var y = 0; y < rows.Length; y++)
                result[x, y] = CreateCreatureBySymbol(rows[y][x], game);
            return result;
        }

        private ICreature CreateCreatureByTypeName(string name, Game game)
        {
            // Это использование механизма рефлексии. 
            // Ему посвящена одна из последних лекций второй части курса Основы программирования
            // В обычном коде можно было обойтись без нее, но нам нужно было написать такой код,
            // который работал бы, даже если вы ещё не создали класс Monster или Gold. 
            // Просто написать new Gold() мы не могли, потому что это не скомпилировалось бы до создания класса Gold.
            if (!factory.ContainsKey(name))
            {
                var type = Assembly
                    .GetExecutingAssembly()
                    .GetTypes()
                    .FirstOrDefault(z => z.Name == name);
                if (type == null)
                    throw new Exception($"Can't find type '{name}'");
                factory[name] = () => (ICreature) Activator.CreateInstance(type, args: game);
            }

            return factory[name]();
        }


        private ICreature CreateCreatureBySymbol(char c, Game game)
        {
            switch (c)
            {
                case 'P':
                    return CreateCreatureByTypeName("Player", game);
                case 'T':
                    return CreateCreatureByTypeName("Terrain", game);
                case 'G':
                    return CreateCreatureByTypeName("Gold", game);
                case 'S':
                    return CreateCreatureByTypeName("Sack", game);
                case 'M':
                    return CreateCreatureByTypeName("Monster", game);
                case ' ':
                    return null;
                default:
                    throw new Exception($"wrong character for ICreature {c}");
            }
        }
    }
}