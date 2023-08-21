import * as THREE from 'three';
import { TrackballControls } from "https://cdn.skypack.dev/three-trackballcontrols-ts@0.2.3";
import { OBJLoader } from 'three/addons/loaders/OBJLoader.js';
import { FBXLoader } from 'three/addons/loaders/FBXLoader';
import { CullFaceNone, MeshStandardMaterial } from 'three';
import {
    std135Aov, rockSceneObjNames, 
    rockSceneMaps, RockPartsPaths
} from './globals.js'; 

// ---------------------------------------------------------------------
// ------------------------------- Settings ----------------------------
// ---------------------------------------------------------------------

// When set to true, use fbx loader, otherwise obj loader 
const useFBX = true;

// When set to false, texture maps will not be loaded 
const useTextures = false; 

// The main model to be loaded 
const modelToUse = null;

// The texture of the main model 
const textureToUse = null;

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
    #modelToUse = null;
    #textureToUse = null;

    #manualMovement = true; 

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

        // Create trackball which allows the user to rotate, pan, zoom 
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
        const intensity2 = 1;
        const light2 = new THREE.DirectionalLight(color, intensity2);
        light2.position.set(0, 2, 0);

        this.#scene.add(light1);
        this.#scene.add(light2); 
    }

    // ---------------------------------------------------------------------
    /* Read and load the objects into the scene
     * NOTE that this is no longder used and some of the variables here
     * will have undefined error if invoked */
    loadGeometries() {
        var scope = this; // Store this reference to be used in functions 

        const textureLoader = new THREE.TextureLoader();
        var pbrMaterial; 
        //if(useTextures)
        //    var pbrMaterial = new MeshStandardMaterial({
        //        map: textureLoader.load(this.#textureToUse.diffuse),
        //        normalMap: textureLoader.load(this.#textureToUse.normal),
        //        normalScale: new THREE.Vector2(1, 1),
        //        emissiveMap: textureLoader.load(this.#textureToUse.diffuse),
        //        emissiveIntensity: .5
        //    });

        var modelLoader = null;
        if (this.#useFBX)
            modelLoader = new FBXLoader();
        else
            modelLoader = new OBJLoader();

        for (let path of this.#modelToUse()) {
            modelLoader.load(path,
                function (obj) {
                    obj.traverse(function (child) {
                        if (child instanceof THREE.Mesh && scope.#useTextures) {
                            child.material = pbrMaterial;
                        }
                    });
                    scope.#scene.add(obj);
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

    // ---------------------------------------------------------------------
    // Read and load the arctic rock scene
    loadGeometriesRock() {
        // There might be better ways to do this?
        // Ref https://discourse.threejs.org/t/assign-and-update-child-mesh-material/14500/4

        var scope = this; // Store this reference to be used in functions 

        var textureLoader = new THREE.TextureLoader();
        var pbrMaterialBG; 
        var pbrMaterialGround; 

        // Material for the background image plane 
        var pbrMaterialBG = new MeshStandardMaterial({
            map: textureLoader.load(rockSceneMaps.BG),
            emissiveMap: textureLoader.load(rockSceneMaps.BG),
            emissiveIntensity: 10.0,
        });

        // Material for the ground (a rock scaled up) 
        var pbrMaterialGround = new MeshStandardMaterial({
            map: textureLoader.load(rockSceneMaps.groundAlb),
            normalMap: textureLoader.load(rockSceneMaps.groundNorm),
            normalScale: new THREE.Vector2(1, 1),
        });

        var pbrMaterialRock = new MeshStandardMaterial({
            map: textureLoader.load(rockSceneMaps.rockAlb),
            normalMap: textureLoader.load(rockSceneMaps.rockNorm),
            normalScale: new THREE.Vector2(1, 1),
        });

        var modelLoader = new OBJLoader();
        modelLoader.load(RockPartsPaths[0], // The background plane 
            function (obj) {
                obj.traverse(function (child) {
                    // Find the BG image plane and assign the corresponding material
                    if (child.isMesh && child.name == rockSceneObjNames.background) {
                        child.material = pbrMaterialBG;
                    }
                });
                scope.#scene.add(obj);
            },
            function (xhr) {
                console.log((xhr.loaded / xhr.total * 100) + "% loaded")
            },
            function (err) {
                console.error("Error loading the object")
            }
        )

        modelLoader = new OBJLoader();
        modelLoader.load(RockPartsPaths[1], // The ground rock 
            function (obj) {
                obj.traverse(function (child) {
                    if (child.isMesh && child.name == rockSceneObjNames.groundRock) {
                        child.material = pbrMaterialGround;
                    }
                });
                scope.#scene.add(obj);
            },
            function (xhr) {
                console.log((xhr.loaded / xhr.total * 100) + "% loaded")
            },
            function (err) {
                console.error("Error loading the object")
            }
        )

    }

    // ---------------------------------------------------------------------
    // Jude if render area needs to be resized and re-rendered 
    resizeRendererToDisplaySize(renderer) {
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

    // ---------------------------------------------------------------------
    // Update the controls, cameras, or the shaders 
    update() {

        var rotation = this.#camera.rotation; 
        
        this.#camera.position.set(-12, -5, -70);
        //-12.532438819599603, y: -5.712040425508086, z: -71.42744566067631
        this.#camera.rotation.set(3, 0, 2);
        //console.log(this.#camera.rotation);

    }

    // ---------------------------------------------------------------------
    // Examine the render area and redraw the canvas  
    redraw = (time) => {
        if (this.resizeRendererToDisplaySize(this.#renderer)) {
            const canvas = this.#renderer.domElement;
            this.#camera.aspect = canvas.clientWidth / canvas.clientHeight;
            this.#camera.updateProjectionMatrix();
        }

        

        requestAnimationFrame(this.redraw); // Context is automatically preserved

        // Manual movement rely on hard coded mouse movement detection 
        if (this.#manualMovement) {
            this.update(); 
        } else {
            this.#controls.update();
            //console.log(this.#camera.rotation)
            //console.log(this.#camera.position)
        }

        this.#renderer.render(this.#scene, this.#camera);
    }


    // ---------------------------------------------------------------------
    // Main function of the class 
    main() {

        this.startup();
        this.addLights();
        this.loadGeometriesRock(); 

        requestAnimationFrame(this.redraw);
    }

}


