
/*
 * Try not to use direct paths and file names in other script files.
 * Use this global file to store all the paths and names, so that if 
 * something changes, this is the only file that need to be checked. 
 */

/* This toturial set could be of help 
 * https://github.com/stemkoski/stemkoski.github.com/tree/master/Three.js/js 
 * */



const testInLocal = true; 

const developerMode = true; 

// ---------------------------------------------------------------------
// -------------------------- Class definition -----------------------
// ---------------------------------------------------------------------
// For ease of use later in the scripts. 


class textureSet {
    constructor(diffuse, normal, roughness, metallic) {
        this.diffuse = diffuse;
        this.normal = normal;
        this.roughness = roughness;
        this.metallic = metallic; 
    }
}

// ---------------------------------------------------------------------
// -------------------------- Explicit variables -----------------------
// ---------------------------------------------------------------------
// Variables that may be used externally when adding the export keyword. 


// Path of the root folder in the server 
const rootPath = "https://odeedelessons.org.ohio-state.edu/U.OSU.EDU/ARVR/WebglEmbed"; 

const modelSubFolder = '/public/models/'; 

const textureMapSubFodler = '/public/textures/';


// ---------------------------------------------------------------------
// -------------------------- Implicit variables -----------------------
// ---------------------------------------------------------------------
// Variables that should not have the export keyword and should only 
// be used within this file for storage or other purposes. 

// const _PartsNamesWhatever 

const _PartsNamesMonkeySkull = [
    'PartBase.obj',
    'PartFrontal.obj',
    'PartNasal.obj',
    'PartParitalLeft.obj',
    'PartParitalRight.obj'
];

export const RockPartsPaths = [
    modelPath('RockScene_P00.obj'), 
    modelPath('RockScene_P01.obj'),
    modelPath('RockScene_P02.obj')
];

export const rockSceneObjNames = {
    groundRock: 'Aset_nature_rock_S_wd3cciw',
    background: 'HughesBluff_Pano_JS',
    rock:   'baked_mesh'
};

export const rockSceneMaps = {
    BG:         mapPath('HughesBluff_Pano_JS.JPG'),
    groundAlb:  mapPath('wd3cciw_2K_Albedo.jpg'), 
    groundNorm: mapPath('wd3cciw_2K_Normal_LOD0.jpg'), 
    groundRoug: mapPath('wd3cciw_2K_Roughness.jpg'),
    rockAlb:    mapPath('baked_mesh_diffuse.png'), 
    rockNorm:   mapPath('baked_mesh_normal.png')
}; 



// ---------------------------------------------------------------------
// ----------------------------- Functions -----------------------------
// ---------------------------------------------------------------------


function modelPath(fileName) {
    // Return the path to the rock scene that consists of 2 parts 
    if (testInLocal)
        return ('.' + modelSubFolder + fileName);
    else
        return (rootPath + modelSubFolder + fileName);
}

function mapPath(fileName) {
    
    if (testInLocal)
        return ('.' + textureMapSubFodler + fileName);
    else
        return (rootPath + textureMapSubFodler + fileName);
    
}

// ---------------------------------------------------------------------
// ------------------------------- MISC --------------------------------

export function std135Aov(focalLength) {
    // Given a focal length in mm, convert focal length into angle of view,
    // assuming the lens is mounted on an standard 135 format still camera. 
    return Math.atan(18 / focalLength) * 2 * (180 / Math.PI);
}

