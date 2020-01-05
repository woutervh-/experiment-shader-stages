const MAX_PATH_LENGTH = 5;

class CanvasContainer {
  constructor(id, ordinal) {
    this.canvas = document.getElementById(id);
    this.context = this.canvas.getContext('2d');
    this.ordinal = ordinal;
  }

  clientToCanvas(p) {
    const bounds = this.canvas.getBoundingClientRect();
    return {
      x: (p.x - bounds.left) * this.canvas.width / bounds.width,
      y: (p.y - bounds.top) * this.canvas.height / bounds.height
    };
  }

  canvasToPlane(p) {
    return {
      x: p.y / this.canvas.height * 2 - 1,
      y: 1 - p.x / this.canvas.width * 2
    };
  }

  planeToCanvas(p) {
    return {
      x: (p.x + 1) / 2 * this.canvas.width,
      y: (1 - p.y) / 2 * this.canvas.height
    };
  }

  renderQuad(p1, p2) {
    const p1Canvas = this.planeToCanvas(p1);
    const p2Canvas = this.planeToCanvas(p2);
    this.context.beginPath();
    this.context.rect(p1Canvas.x, p1Canvas.y, p2Canvas.x - p1Canvas.x, p2Canvas.y - p1Canvas.y);
    this.context.closePath();
  }

  renderLine(p1, p2) {
    const p1Canvas = this.planeToCanvas(p1);
    const p2Canvas = this.planeToCanvas(p2);
    this.context.beginPath();
    this.context.moveTo(p1Canvas.x, p1Canvas.y);
    this.context.lineTo(p2Canvas.x, p2Canvas.y);
    this.context.closePath();
  }

  renderPoint(p, r) {
    const pCanvas = this.planeToCanvas(p);
    this.context.beginPath();
    this.context.arc(pCanvas.x, pCanvas.y, r, 0, 2 * Math.PI);
    this.context.closePath();
  }

  drawAxes() {
    this.context.lineWidth = 4;
    this.context.strokeStyle = 'red';
    this.renderLine({ x: 0, y: 0 }, { x: 0.1, y: 0 });
    this.context.stroke();
    this.context.strokeStyle = 'green';
    this.renderLine({ x: 0, y: 0 }, { x: 0, y: 0.1 });
    this.context.stroke();

    // this.renderPoint({ x: -1, y: -1 }, 10);
    // this.context.stroke();
    // this.context.strokeStyle = 'green';
    // this.renderPoint({ x: 1, y: -1 }, 10);
    // this.context.stroke();
    // this.context.strokeStyle = 'blue';
    // this.renderPoint({ x: -1, y: 1 }, 10);
    // this.context.stroke();
  }

  renderLeaf(path) {
    let offset = { x: 0, y: 0 };
    let scale = 1;
    for (const childIndex of path) {
      const childOffset = childOffsetVectors[childIndex];
      scale /= 2;
      offset = vadd(offset, vmultiply(childOffset, scale));
    }
    this.renderNode(offset, scale);
  }

  renderNode(offset, scale) {
    const p1 = vadd(offset, vmultiply({ x: -1, y: -1 }, scale));
    const p2 = vadd(offset, vmultiply({ x: 1, y: 1 }, scale));
    this.renderQuad(p1, p2);
  }

  renderPath(path) {
    let offset = { x: 0, y: 0 };
    let scale = 1;
    this.renderNode(offset, scale);
    for (const childIndex of path) {
      const childOffset = childOffsetVectors[childIndex];
      scale /= 2;
      offset = vadd(offset, vmultiply(childOffset, scale));
      this.renderNode(offset, scale);
    }
  }
}

const containers = [
  new CanvasContainer('up', 0), // up
  new CanvasContainer('down', 1), // down
  new CanvasContainer('left', 2), // left
  new CanvasContainer('right', 3), // right
  new CanvasContainer('forward', 4), // forward
  new CanvasContainer('back', 5) // back
];

const vadd = (p1, p2) => {
  return { x: p1.x + p2.x, y: p1.y + p2.y };
};

const vsubtract = (p1, p2) => {
  return { x: p1.x - p2.x, y: p1.y - p2.y };
};

const vmultiply = (p, s) => {
  return { x: p.x * s, y: p.y * s };
};

const vdivide = (p, s) => {
  return { x: p.x / s, y: p.y / s };
};

const getChildIndex = (pointInPlane, offset, scale) => {
  const t = vdivide(vsubtract(pointInPlane, offset), scale);
  return (t.x < 0 ? 0 : 1) + (t.y < 0 ? 0 : 2);
};

const childOffsetVectors = [
  { x: -1, y: -1 },
  { x: 1, y: -1 },
  { x: -1, y: 1 },
  { x: 1, y: 1 }
];

const getDeepPath = (pointInPlane) => {
  const path = [];
  let offset = { x: 0, y: 0 };
  let scale = 1;
  while (path.length < MAX_PATH_LENGTH) {
    const childIndex = getChildIndex(pointInPlane, offset, scale)
    const childOffset = childOffsetVectors[childIndex];
    scale /= 2;
    offset = vadd(offset, vmultiply(childOffset, scale));
    path.push(childIndex);
  }
  return path;
};

const getParentPath = (path) => {
  return path.slice(0, path.length - 1);
};

const neighborSameParent = [
  [false, true, false, true],
  [true, false, false, true],
  [false, true, true, false],
  [true, false, true, false]
];

const neighborOrdinal = [
  [1, 1, 2, 2],
  [0, 0, 3, 3],
  [3, 3, 0, 0],
  [2, 2, 1, 1]
];

const rootOrdinalRotation = [
  [2, 3, 4, 5],
  [3, 2, 4, 5],
  [4, 5, 0, 1],
  [5, 4, 0, 1],
  [1, 0, 3, 2],
  [0, 1, 3, 2]
];

const neighborOrdinalRotation = [
  [
    [0, 1, 2, 3],
    [0, 1, 2, 3],
    [0, 2, 1, 3],
    [3, 1, 2, 0],
    [3, 1, 2, 0],
    [3, 1, 2, 0]
  ],
  [
    [0, 1, 2, 3],
    [0, 1, 2, 3],
    [0, 2, 1, 3],
    [3, 1, 2, 0],
    [0, 2, 1, 3],
    [0, 2, 1, 3]
  ],
  [
    [0, 2, 1, 3],
    [0, 2, 1, 3],
    [0, 1, 2, 3],
    [0, 1, 2, 3],
    [3, 1, 2, 0],
    [0, 2, 1, 3]
  ],
  [
    [3, 1, 2, 0],
    [3, 1, 2, 0],
    [0, 1, 2, 3],
    [0, 1, 2, 3],
    [3, 1, 2, 0],
    [0, 2, 1, 3]
  ],
  [
    [3, 1, 2, 0],
    [0, 2, 1, 3],
    [3, 1, 2, 0],
    [3, 1, 2, 0],
    [0, 1, 2, 3],
    [0, 1, 2, 3]
  ],
  [
    [3, 1, 2, 0],
    [0, 2, 1, 3],
    [0, 2, 1, 3],
    [0, 2, 1, 3],
    [0, 1, 2, 3],
    [0, 1, 2, 3]
  ]
];

const getNeighborPath = (path, direction) => {
  const neighborPath = [...path];
  for (let i = path.length - 1; i >= 0; i--) {
    neighborPath[i] = neighborOrdinal[path[i]][direction];
    if (neighborSameParent[path[i]][direction]) {
      break;
    }
  }
  return neighborPath;
};

const getNeighborPathOrdinal = (path, direction) => {
  let commonAncestorDistance = 0;
  for (let i = path.length - 1; i >= 1; i--) {
    if (neighborSameParent[path[i]][direction]) {
      break;
    }
    commonAncestorDistance += 1;
  }
  
  if (commonAncestorDistance < path.length - 1) {
    const neighborPath = [...path];
    for (let i = 0; i < commonAncestorDistance + 1; i++) {
      neighborPath[path.length - i - 1] = neighborOrdinal[path[path.length - i - 1]][direction];
    }
    return neighborPath;
  } else {
    const fromOrdinal = path[0];
    const toOrdinal = rootOrdinalRotation[fromOrdinal][direction];
    return [toOrdinal, ...path.slice(1).map(ordinal => neighborOrdinalRotation[fromOrdinal][toOrdinal][ordinal])];
  }
};

const pathEquals = (path1, path2) => {
  if (path1.length !== path2.length) {
    return false;
  }
  for (let i = 0; i < path1.length; i++) {
    if (path1[i] !== path2[i]) {
      return false;
    }
  }
  return true;
};

const getBalancedPathsOrdinal = (path) => {
  const remaining = [path];
  const done = [];
  while (remaining.length >= 1) {
    const path = remaining.pop();
    if (path.length <= 2) {
      continue;
    }
    const parentPath = getParentPath(path);
    const parentNeighbors = [
      getNeighborPathOrdinal(parentPath, 0),
      getNeighborPathOrdinal(parentPath, 1),
      getNeighborPathOrdinal(parentPath, 2),
      getNeighborPathOrdinal(parentPath, 3)
    ];
    for (const neighbor of parentNeighbors) {
      if (done.every(p => !pathEquals(p, neighbor)) && remaining.every(p => !pathEquals(p, neighbor))) {
        remaining.push(neighbor);
      }
    }
    done.push(path);
  }
  return done;
};

const getBalancedPaths = (path) => {
  const remaining = [path];
  const done = [];
  while (remaining.length >= 1) {
    const path = remaining.pop();
    if (path.length <= 1) {
      continue;
    }
    const parent = getParentPath(path);
    const parentNeighbors = [
      getNeighborPath(parent, 0),
      getNeighborPath(parent, 1),
      getNeighborPath(parent, 2),
      getNeighborPath(parent, 3)
    ];
    for (const neighbor of parentNeighbors) {
      if (done.every(p => !pathEquals(p, neighbor)) && remaining.every(p => !pathEquals(p, neighbor))) {
        remaining.push(neighbor);
      }
    }
    done.push(path);
  }
  return done;
};

const getParentNode = (node) => {
  if (node.parent === null) {
    node.parent = { parent: null, children: [null, null, null, null], path: getParentPath(node.path) };
    node.parent.children[node.path[node.path.length - 1]] = node;
  }
  return node.parent;
};

const getChildNode = (node, ordinal) => {
  if (node.children[ordinal] === null) {
    node.children[ordinal] = { parent: node, children: [null, null, null, null], path: [...node.path, ordinal] };
  }
  return node.children[ordinal];
};

const getNeighborNode = (node, direction) => {
  const ordinal = neighborOrdinal[node.path[node.path.length - 1]][direction];
  const parent = getParentNode(node);
  if (parent.path.length <= 0 || neighborSameParent[node.path[node.path.length - 1]][direction]) {
    return getChildNode(parent, ordinal);
  } else {
    const parentNeighbor = getNeighborNode(parent, direction);
    return getChildNode(parentNeighbor, ordinal);
  }
};

const getBalancedNodes = (path) => {
  const remaining = [{ parent: null, children: [null, null, null, null], path }];
  const done = [];
  while (remaining.length >= 1) {
    const node = remaining.pop();
    if (node.path.length <= 1) {
      continue;
    }
    const parent = getParentNode(node);
    const parentNeighbors = [
      getNeighborNode(parent, 0),
      getNeighborNode(parent, 1),
      getNeighborNode(parent, 2),
      getNeighborNode(parent, 3)
    ];
    for (const neighbor of parentNeighbors) {
      if (!done.includes(neighbor) && !remaining.includes(neighbor)) {
        remaining.push(neighbor);
      }
    }
    done.push(node);
  }
  return done;
};

const nodesToPaths = (nodes) => nodes.map(node => node.path);

const update = (canvasContainer, pointInPlane) => {
  canvasContainer.context.clearRect(0, 0, canvasContainer.canvas.width, canvasContainer.canvas.height);

  const path = getDeepPath(pointInPlane);
  const balancedNodes = getBalancedNodes(path);
  const balancedPaths = nodesToPaths(balancedNodes);
  // const balancedPaths = getBalancedPaths(path);

  canvasContainer.context.strokeStyle = 'black';
  canvasContainer.context.lineWidth = 1;
  for (const path of balancedPaths) {
    canvasContainer.renderLeaf(path);
  }

  canvasContainer.context.strokeStyle = 'black';
  canvasContainer.context.lineWidth = 2;
  canvasContainer.renderLeaf(path);
};

const updateOrdinal = (pointInClient) => {
  for (const container of containers) {
    container.context.clearRect(0, 0, container.canvas.width, container.canvas.height);
    container.drawAxes();
  }

  const pointInCanvas = containers[5].clientToCanvas(pointInClient);
  const pointInPlane = containers[5].canvasToPlane(pointInCanvas);
  const path = [5, ...getDeepPath(pointInPlane)];
  const balancedPaths = getBalancedPathsOrdinal(path);

  containers[path[0]].context.fillStyle = 'white';
  containers[path[0]].renderLeaf(path.slice(1));
  containers[path[0]].context.fill();

  for (const container of containers) {
    container.context.strokeStyle = 'black';
    container.context.lineWidth = 1;
  }
  for (const path of balancedPaths) {
    containers[path[0]].renderLeaf(path.slice(1));
    containers[path[0]].context.stroke();
  }
};

document.addEventListener('mousemove', (event) => {
  const x = event.clientX;
  const y = event.clientY;
  const pointInClient = { x: event.clientX, y: event.clientY };
  // const pointInCanvas = back.clientToCanvas(pointInClient);
  // const pointInPlane = back.canvasToPlane(pointInCanvas);
  // update(back, pointInPlane);
  updateOrdinal(pointInClient);
});