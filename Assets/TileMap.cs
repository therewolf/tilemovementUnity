using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TileMap : MonoBehaviour {

	public GameObject selectedUnit;


	public TileType[] tileTypes;

	int[,] tiles;
	Node[,] graph;


	int mapSizeX = 10;
	int mapSizeY = 10;

	void Start() {

		selectedUnit.GetComponent<Unit> ().tileX = (int)selectedUnit.transform.position.x;
		selectedUnit.GetComponent<Unit> ().tileY = (int)selectedUnit.transform.position.y;
		selectedUnit.GetComponent<Unit> ().map = this;


		GenerateMapData ();
		GeneratePathfindingGraph ();
		GenerateMapVisual ();
	}

	void GenerateMapData(){
		// Allocate map tiles
		tiles = new int[mapSizeX, mapSizeY];

		int x, y;

		// Initialize map tiles to grass
		for (x = 0; x < mapSizeX; x++) {
			for (y = 0; y < mapSizeY; y++) {
				tiles [x, y] = 0;
			}
		}

		for(x=3; x<=5; x++) {
			for(y=0; y<4; y++) {
				tiles[x,y] = 1;
			}
		}

		// Mountain range?
		tiles[4,4]  = 2;
		tiles[5,4]  = 2;
		tiles[6,4]  = 2;
		tiles[7,4]  = 2;
		tiles[8,4]  = 2;

		tiles[4,5]  = 2;
		tiles[4,6]  = 2;
		tiles[8,5]  = 2;
		tiles[8,6]  = 2;
	}

	float CostToEnterTile(int x, int y){
		//if remaining movement rounded allows for entry then it is allowed
	}

	void GeneratePathfindingGraph(){

		//Init array
		graph = new Node[mapSizeX, mapSizeY];
		//Init node for each spot in array
		for (int x = 0; x < mapSizeX; x++) {
			for (int y = 0; y < mapSizeY; y++) {
				graph [x, y] = new Node ();
				graph [x, y].x = x;
				graph [x, y].y = y;
			}
		}

		//now that all nodes xist, calculate neighbours
		for (int x = 0; x < mapSizeX; x++) {
			for (int y = 0; y < mapSizeY; y++) {



				// We have a 4-way connected map
				// This also works with 6-way hexes and 8-way tiles and n-way variable areas (Like EU4)
				if(x > 0)
					graph [x, y].neighbours.Add (graph [x - 1, y]);
				if(x < mapSizeX-1)
					graph [x, y].neighbours.Add (graph [x + 1, y]);
				if(y > 0)
					graph [x, y].neighbours.Add (graph [x, y - 1]);
				if(y < mapSizeY-1)
					graph [x, y].neighbours.Add (graph [x, y + 1]);
			}
		}
	}

	void GenerateMapVisual(){
		for (int x = 0; x < mapSizeX; x++) {
			for (int y = 0; y < mapSizeY; y++) {
				TileType tt = tileTypes [tiles [x, y]];

				GameObject go = (GameObject)Instantiate (tt.tileVisualPrefab, new Vector3 (x, y, 0), Quaternion.identity);

				ClickableTile ct = go.GetComponent<ClickableTile> ();
				ct.tileX = x;
				ct.tileY = y;
				ct.map = this;
			}
		}

	}

	public Vector3 TileCoordToWorldCoord(int x, int y){
		return new Vector3 (x, y, 0);
	}

	public void GeneratePathTo(int x, int y) {
		//clear out old path
		selectedUnit.GetComponent<Unit> ().currentPath = null;

		Dictionary<Node, float> dist = new Dictionary<Node, float> ();
		Dictionary<Node, Node> prev = new Dictionary<Node, Node> ();

		//List of nodes to be checked
		List<Node> unvisited = new List<Node> ();

		Node source = graph [
			              selectedUnit.GetComponent<Unit> ().tileX,
			              selectedUnit.GetComponent<Unit> ().tileX
		              ];

		Node target = graph [
			              x,
			              y
		              ];

		dist [source] = 0;
		prev [source] = null;
		// Init everything to have INFINITY distance, since
		// we don't know any better right now. Also, it's possible
		// that some nodes can't be reached from the source,
		// which would make INFINITY a reasonable value
		foreach (Node v in graph) {
			if (v != source) {
				dist [v] = Mathf.Infinity;
				prev [v] = null;
			}

			unvisited.Add (v);
		}

		while (unvisited.Count > 0) {
			Node u = null;

			foreach (Node possibleU in unvisited) {
				if (u == null || dist[possibleU] < dist[u]) {
					u = possibleU;
				}
			}

			if (u == target) {
				break; //exit while
			}

			unvisited.Remove (u);

			foreach (Node v in u.neighbours) {
				float alt = dist [u] + u.DistanceTo (v);
				if (alt < dist [v]) {
					dist [v] = alt;
					prev [v] = u;
				}
			}
		}

		if (prev[target] == null) {
			return;
		}

		List<Node> currentPath = new List<Node> ();

		Node curr = target;

		while (curr != null) {
			currentPath.Add (curr);
			curr = prev [curr];
		}

		currentPath.Reverse ();

		selectedUnit.GetComponent<Unit> ().currentPath = currentPath;
	}
}