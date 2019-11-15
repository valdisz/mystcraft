using System.Collections.Generic;

namespace atlantis {
    public class Edge {
        public Edge(Node source, Node target, string direction, int dificulity) {
            Source = source;
            Target = target;
            Direction = direction;
            Dificulity = dificulity;
        }

        public int Dificulity { get; }
        public string Direction { get; }
        public Node Source { get; }
        public Node Target { get; }

        public static Edge N(Node source, Node target, int dificulity) {
            return new Edge(source, target, "N", dificulity);
        }

        public static Edge NW(Node source, Node target, int dificulity) {
            return new Edge(source, target, "NW", dificulity);
        }

        public static Edge NE(Node source, Node target, int dificulity) {
            return new Edge(source, target, "NE", dificulity);
        }

        public static Edge S(Node source, Node target, int dificulity) {
            return new Edge(source, target, "S", dificulity);
        }

        public static Edge SW(Node source, Node target, int dificulity) {
            return new Edge(source, target, "SW", dificulity);
        }

        public static Edge SE(Node source, Node target, int dificulity) {
            return new Edge(source, target, "SE", dificulity);
        }
    }

    public class Node {
        public IList<Edge> Edges { get; } = new List<Edge>();

    }

    public class RegionNode {

    }


    public class Map {
        public Map() {
            var r1 = new Node();
            var r2 = new Node();
            var r3 = new Node();
            var r4 = new Node();

            Edge.N(r1, r2, 1);
            Edge.S(r2, r1, 1);
        }
    }
}
