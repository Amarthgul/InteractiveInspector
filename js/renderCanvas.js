import * as THREE from 'three';
import { TrackballControls } from "https://cdn.skypack.dev/three-trackballcontrols-ts@0.2.3";
import { OBJLoader } from 'three/addons/loaders/OBJLoader.js';
import { FBXLoader } from 'three/addons/loaders/FBXLoader';
import { CullFaceNone, MeshStandardMaterial } from 'three';
import {
    std135Aov, getRockScene00PartsPaths, 
    getMonkeySkullPartsPaths, MonkeySkullMaps
} from './globals.js'; 

// ---------------------------------------------------------------------
// ------------------------------- Settings ----------------------------
// ---------------------------------------------------------------------

// When set to true, use fbx loader, otherwise obj loader 
const useFBX = true;

// When set to false, texture maps will not be loaded 
const useTextures = false; 

// The main model to be loaded 
const modelToUse = getRockScene00PartsPaths;

// The texture of the main model 
const textureToUse = MonkeySkullMaps;

// ---------------------------------------------------------------------
// ------------------------------ Functions ----------------------------
// ---------------------------------------------------------------------

export class RenderCanvas {

    #canvas;             // The primary element of the rendering target 
    #renderer;           // WebgL renderer

    #backgroundColor = 0x000000;
    #backgroundOpacity = 0; 

    #scene; 

    #camera;             // Main camera 
    #camera135FocalLength = 50; // 50mm lens 
    #cameraAoV;          // Field of view in degree 
    #cameraAspect = 2;   // Width / height ratio
    #cameraNear = 0.1;   // Near clipping plane 
    #cameraFar = 2000;   // Far clipping plane

    #controls;           // Trackball interactive control 

    #useFBX = true;
    #useTextures = false; 
    #modelToUse = getRockScene00PartsPaths;
    #textureToUse = MonkeySkullMaps;

    // ---------------------------------------------------------------------
    // Class constructor 
    constructor(canvasID, parentID) {
        this.#canvas = document.querySelector(canvasID);
        this.#renderer = new THREE.WebGLRenderer({
            antialias: true,
            canvas: this.#canvas
        });

        this.#renderer.setClearColor(this.#backgroundColor, this.#backgroundOpacity);
        document.getElementById(parentID).appendChild(this.#renderer.domElement); 
    }

    // ---------------------------------------------------------------------
    // Create the scene, set up initial camera 
    startup() {

        // Set up the camera 
        this.#cameraAoV = std135Aov(this.#camera135FocalLength);
        this.#camera = new THREE.PerspectiveCamera(
            this.#cameraAoV,
            this.#cameraAspect,
            this.#cameraNear,
            this.#cameraFar);
        this.#camera.position.z = 100;

        this.#scene = new THREE.Scene();

        this.#controls = new TrackballControls(this.#camera, this.#renderer.domElement);
        this.#controls.rotateSpeed = 4;
        this.#controls.dynamicDampingFactor = 0.1;
    } 

    // ---------------------------------------------------------------------
    // Create and add lights into the scene, call only after startup()
    addLights() {
        const color = 0xFFFFFF;
        const intensity1 = 1;
        const light1 = new THREE.DirectionalLight(color, intensity1);
        light1.position.set(-1, 2, 4);
        const intensity2 = 0.75;
        const light2 = new THREE.DirectionalLight(color, intensity2);
        light2.position.set(1, -2, -4);

        this.#scene.add(light1);
        this.#scene.add(light2); 
    }

    // ---------------------------------------------------------------------
    // Read and load the objects into the scene 
    loadGeometries() {

        const textureLoader = new THREE.TextureLoader();
        const pbrMaterial = new MeshStandardMaterial({
            map: textureLoader.load(this.#textureToUse.diffuse),
            normalMap: textureLoader.load(this.#textureToUse.normal),
            normalScale: new THREE.Vector2(1, 1),
            emissiveMap: textureLoader.load(this.#textureToUse.diffuse),
            emissiveIntensity: .5
        });

        var modelLoader = null;
        if (this.#useFBX)
            modelLoader = new FBXLoader();
        else
            modelLoader = new OBJLoader();

        for (let path of this.#modelToUse()) {
            modelLoader.load(path,
                function (obj) {
                    obj.traverse(function (child) {
                        //if (child instanceof THREE.Mesh && this.#useTextures) {
                        //    child.material = pbrMaterial;
                        //}
                    });
                    this.#scene.add(obj);
                },
                function (xhr) {
                    //console.log((xhr.loaded / xhr.total * 100) + "% loaded")
                },
                function (err) {
                    console.error("Error loading the object")
                }
            )
        }
    }

    update = (time) => {
        const canvas = this.#renderer.domElement;
        this.#camera.aspect = canvas.clientWidth / canvas.clientHeight;
        this.#camera.updateProjectionMatrix();

        requestAnimationFrame(this.update); // Context is automatically preserved
        this.#controls.update();
        this.#renderer.render(this.#scene, this.#camera);
    }


    // ---------------------------------------------------------------------
    // Main function of the class 
    main() {

        this.startup();
        this.addLights();
        this.loadGeometries(); 

        requestAnimationFrame(this.update);
    }

}


function resizeRendererToDisplaySize(renderer) {
    // Resize the render area to accommodate the change of resoultion
    const canvas = renderer.domElement;
    const pixelRatio = window.devicePixelRatio;
    const width = canvas.clientWidth * pixelRatio | 0;
    const height = canvas.clientHeight * pixelRatio | 0;
    const needResize = canvas.width !== width || canvas.height !== height;
    if (needResize) {
        renderer.setSize(width, height, false);
    }
    return needResize;
}



export function renderCanvasMain() {

    // Accquiring the element and set it as the render target 
    const canvas = document.querySelector('#c');
    const renderer = new THREE.WebGLRenderer({ antialias: true, canvas });

    renderer.setClearColor(0x000000, 0); // Turn background transparent 
    document.getElementById("mainContainer").appendChild(renderer.domElement); // To use later for trackball 

	// Set up the camera 
    const fov = std135Aov(50);
    const aspect = 2;  // the canvas default
    const near = 0.1;
    const far = 2000;
    const camera = new THREE.PerspectiveCamera(fov, aspect, near, far);
    camera.position.z = 100;

	// Create the lights 
    const color = 0xFFFFFF;
    const intensity1 = 1;
    const light1 = new THREE.DirectionalLight(color, intensity1);
    light1.position.set(-1, 2, 4);
    const intensity2 = 0.75;
    const light2 = new THREE.DirectionalLight(color, intensity2);
    light2.position.set(1, -2, -4);

	// Create the scene and add the object and light 
    const scene = new THREE.Scene();
    scene.add(light1); 
    scene.add(light2); 

    // Load all the objects 
    const textureLoader = new THREE.TextureLoader();
    const pbrMaterial = new MeshStandardMaterial({
        map: textureLoader.load(MonkeySkullMaps.diffuse),
        normalMap: textureLoader.load(MonkeySkullMaps.normal),
        normalScale: new THREE.Vector2(1, 1),
        emissiveMap: textureLoader.load(MonkeySkullMaps.diffuse),
        emissiveIntensity: .5
    });

    var modelLoader = null; 
    if (useFBX)
        modelLoader = new FBXLoader();
    else
        modelLoader = new OBJLoader();

    for (let path of modelToUse()) {
        modelLoader.load(path,
            function (obj) {
                obj.traverse(function (child) {
                    if (child instanceof THREE.Mesh && useTextures) {
                        child.material = pbrMaterial;
                    }
                });
                scene.add(obj);
            },
            function (xhr) {
                console.log((xhr.loaded / xhr.total * 100) + "% loaded")
            },
            function (err) {
                console.error("Error loading 'ship.obj'")
            }
        )
    }
	

	// Create the trackball control 
	const controls = new TrackballControls(camera, renderer.domElement); 
	controls.rotateSpeed = 4;
	controls.dynamicDampingFactor = 0.1;
		
	
    function Update(time) {

        //const canvas = renderer.domElement;
        //camera.aspect = canvas.clientWidth / canvas.clientHeight;
        //camera.updateProjectionMatrix();

        if (resizeRendererToDisplaySize(renderer)) {
            const canvas = renderer.domElement;
            camera.aspect = canvas.clientWidth / canvas.clientHeight;
            camera.updateProjectionMatrix();
        }
		
        requestAnimationFrame(Update);
		controls.update();
		
		renderer.render(scene, camera);
    }

    requestAnimationFrame(Update);
}