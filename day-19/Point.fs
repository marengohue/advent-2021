namespace Advent.Day19

// All coordinates are following the right-hand rule:
// Positive X goes right
// Positive Y goes up
// Positive Z goes into the observer
// All rotations are counter-clockwise
          
module Point =
    module Point2 =
        type Point2 = int*int
        type RotationFn2 = Point2 -> Point2

        let (@+) ((x1, y1) : Point2) ((x2, y2) : Point2) = Point2(x1 + x2, y1 + y2)
        let (@-) ((x1, y1) : Point2) ((x2, y2) : Point2) = Point2(x1 - x2, y1 - y2)

        let enumerateRotationFns : (string*RotationFn2) array =
            [|
                "+000deg, up is +Z", id
                "+090deg, up is +Z", fun (x, y) -> Point2(-y, +x);
                "+180deg, up is +Z", fun (x, y) -> Point2(-x, -y);
                "+270deg, up is +Z", fun (x, y) -> Point2(+y, -x);
                "+000deg, up is -Z", fun (x, y) -> Point2(+x, -y);
                "+090deg, up is -Z", fun (x, y) -> Point2(+y, +x);
                "+180deg, up is -Z", fun (x, y) -> Point2(-x, +y);
                "+270deg, up is -Z", fun (x, y) -> Point2(-y, -x);
            |]
            
    module Point3 =
        type Point3 = int*int*int
        type RotationFn3 = Point3 -> Point3

        let (@+) ((x1, y1, z1) : Point3) ((x2, y2, z2) : Point3) =
            Point3(x1 + x2, y1 + y2, z1 + z2)
            
        let (@-) ((x1, y1, z1) : Point3) ((x2, y2, z2) : Point3) =
            Point3(x1 - x2, y1 - y2, z1 - z2)

        let manhattan ((x, y, z) : Point3) : int = abs x + abs y + abs z
        
        let enumerateRotationFns : (string*RotationFn3) array =
            [|
                "X is +X, 000deg X rot", id;
                "X is +X, 090deg X rot", fun (x, y, z) -> Point3(+x, -z, +y);
                "X is +X, 180deg X rot", fun (x, y, z) -> Point3(+x, -y, -z);
                "X is +X, 270deg X rot", fun (x, y, z) -> Point3(+x, +z, -y);
             
                "X is -X, 000deg X rot", fun (x, y, z) -> Point3(-x, +y, -z);
                "X is -X, 090deg X rot", fun (x, y, z) -> Point3(-x, +z, +y);
                "X is -X, 180deg X rot", fun (x, y, z) -> Point3(-x, -y, +z);
                "X is -X, 270deg X rot", fun (x, y, z) -> Point3(-x, -z, -y);

                "Y is +X, 000deg Y rot", fun (x, y, z) -> Point3(+y, +z, +x);
                "Y is +X, 090deg Y rot", fun (x, y, z) -> Point3(+y, -x, +z);
                "Y is +X, 180deg Y rot", fun (x, y, z) -> Point3(+y, -z, -x);
                "Y is +X, 000deg Y rot", fun (x, y, z) -> Point3(+y, +x, -z);

                "Y is -X, 000deg Y rot", fun (x, y, z) -> Point3(-y, +z, -x);
                "Y is -X, 090deg Y rot", fun (x, y, z) -> Point3(-y, +x, +z);
                "Y is -X, 180deg Y rot", fun (x, y, z) -> Point3(-y, -z, +x);
                "Y is -X, 270deg Y rot", fun (x, y, z) -> Point3(-y, -x, -z);

                "Z is +X, 000deg Z rot", fun (x, y, z) -> Point3(+z, +x, +y);
                "Z is +X, 090deg Z rot", fun (x, y, z) -> Point3(+z, -y, +x);
                "Z is +X, 180deg Z rot", fun (x, y, z) -> Point3(+z, -x, -y);
                "Z is +X, 270deg Z rot", fun (x, y, z) -> Point3(+z, +y, -x);

                "Z is -X, 000deg Z rot", fun (x, y, z) -> Point3(-z, +x, -y);
                "Z is -X, 090deg Z rot", fun (x, y, z) -> Point3(-z, +y, +x);
                "Z is -X, 180deg Z rot", fun (x, y, z) -> Point3(-z, -x, +y);
                "Z is -X, 270deg Z rot", fun (x, y, z) -> Point3(-z, -y, -x);
             |]
