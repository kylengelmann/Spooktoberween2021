bl_info = {
    "name": "Spooktoberween",
    "blender": (3, 1, 2),
    "category": "Render",
}

import bpy
from bpy.props import (
    StringProperty,
)

import mathutils
from mathutils import Vector
from math import radians

import os

class SpoopPixelRenderer(bpy.types.Operator):

    bl_idname = "spoop.render_pixels"

    bl_label = "Render Pixels"
    
    directory: StringProperty(
        name = "Directory Path",
        maxlen=1024,
        subtype = 'DIR_PATH'
    )
    
    def invoke(self, context, _event):

        if not self.directory:
            self.directory = os.path.expanduser("~")
        
        context.window_manager.fileselect_add(self)
        return{'RUNNING_MODAL'}

    def execute(self, context):        
        if len(context.selected_objects) == 0:
            msg = "Please select the objects to be rendered"
            self.report({'WARNING'}, msg)
            return{'FINISHED'}
        
        userpath = self.properties.directory
        if not os.path.isdir(userpath):
            msg = "Please select a directory not a file\n" + userpath
            self.report({'WARNING'}, msg)
            return{'FINISHED'}
        
        selected_objects = context.selected_objects.copy()
            
        bpy.ops.object.camera_add(location=(0,0,0), rotation=(radians(60),0,radians(-45)))
        cam = context.active_object
        camForward = cam.matrix_world @ Vector((0,0,-1))
        camUp = cam.matrix_world @ Vector((0,1,0))
        
        cam.data.type = 'ORTHO'
        cam.data.ortho_scale = 7.5
        
        context.scene.render.resolution_x = 480
        context.scene.render.resolution_y = 480
        
        context.scene.camera = cam

        spoopLibPath = os.path.join(os.path.dirname(__file__), "Spooktoberween.blend")
        if not os.path.exists(spoopLibPath):
            self.report({'ERROR'}, "Missing asset library, addon installed wrong")

        normalMat = None
        with bpy.data.libraries.load(spoopLibPath, assets_only=True) as (data_from, data_to):
            data_to.materials = data_from.materials

        normalMat = data_to.materials[0]
        print (normalMat)

        for obj in selected_objects:
            if obj.type != 'MESH':
                continue
            
            if obj.data.materials:
                obj.data.materials[0] = normalMat
            else:
                obj.data.materials.append(normalMat)

            renderFileName = os.path.join(userpath, obj.name + ".png")
            print (renderFileName)
            
            camLocation = obj.location - camForward * 10 + camUp * 3.75
            print (str(obj.location) + " | " + str(camLocation))
            
            cam.location = camLocation
            
            context.scene.render.filepath = renderFileName
            bpy.ops.render.render(write_still=True)
            
        bpy.ops.object.delete()
            
        return{'FINISHED'}

def menu_func_import(self, context):
    self.layout.operator(SpoopPixelRenderer.bl_idname, text="Render Pixels")

def register():
    bpy.utils.register_class(SpoopPixelRenderer)
    bpy.types.TOPBAR_MT_render.append(menu_func_import)

def unregister():
    bpy.utils.unregister_class(SpoopPixelRenderer)
    bpy.types.TOPBAR_MT_render.remove(menu_func_import)

if __name__ == "__main__":
    register()