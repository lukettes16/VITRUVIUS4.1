using UnityEngine;
using System.Collections;

public class THC1_ctrl : MonoBehaviour {

    private Animator anim;
    private CharacterController controller;
    private int battle_state = 0;
    public float speed = 6.0f;
    public float runSpeed = 3.0f;
    public float turnSpeed = 60.0f;
    public float gravity = 20.0f;
    private Vector3 moveDirection = Vector3.zero;
    private float r_sp = 0.0f;

    void Start ()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController> ();
        if (controller == null)
        {
            
        }

        r_sp = runSpeed;
    }

    void Update ()
    {
        if (controller == null)
        {
            return;
        }

        if (Input.GetKey ("1"))
        {
            anim.SetInteger ("battle", 0);
            battle_state = 0;
        }
        if (Input.GetKey ("2"))
        {
            anim.SetInteger ("battle", 1);
            battle_state = 1;

        }
        if (Input.GetKey ("3"))
        {
            anim.SetInteger ("battle", 2);
            battle_state = 2;

        }
        if (Input.GetKey ("4"))
        {
            anim.SetInteger ("battle", 3);
            battle_state = 3;

        }

        if (Input.GetKey ("up"))
        {
            if (battle_state == 0) {
                anim.SetInteger ("moving", 1);
                runSpeed = 1;
            }

            if (battle_state == 1) {
                anim.SetInteger ("moving", 2);
                runSpeed = r_sp;
            }
            if (battle_state == 2) {
                anim.SetInteger ("moving", 3);
                runSpeed = 0.66f;
            }
            if (battle_state == 3) {

                runSpeed = 0;
            }
        }
        else
            {
                anim.SetInteger ("moving", 0);
            }

        if (Input.GetMouseButtonDown (0)) {
            anim.SetInteger ("moving", 4);
        }
        if (Input.GetMouseButtonDown (1)) {
            anim.SetInteger ("moving", 5);
        }
        if (Input.GetMouseButtonDown (2)) {
            anim.SetInteger ("moving", 6);
        }

        if (Input.GetKeyDown ("i"))
        {
            anim.SetInteger ("moving", 13);
        }

        if (Input.GetKeyDown ("o"))
        {
            anim.SetInteger ("moving", 12);
        }

        if (Input.GetKeyDown ("u"))
        {
            int n = Random.Range (0, 2);
            if (n == 0)
            {
                anim.SetInteger ("moving", 10);
            }
            else
            {
            anim.SetInteger ("moving", 11);
            }
        }

        if (Input.GetKeyDown ("p")) {
            anim.SetInteger ("moving", 14);
        }
        if (Input.GetKeyUp ("p")) {
            anim.SetInteger ("moving", 15);
        }

        if (Input.GetKeyDown ("z")) {
            anim.SetInteger ("moving", 17);
        }
        if (Input.GetKeyUp ("z")) {
            anim.SetInteger ("moving", 0);
        }

        if (Input.GetKeyDown ("x")) {
            anim.SetInteger ("moving",7);
        }
        if (Input.GetKeyDown ("c")) {
            anim.SetInteger ("moving", 8);
        }
        if (Input.GetKeyDown ("space")) {
            anim.SetInteger ("moving", 16);
        }

        if (Input.GetKeyDown ("v")) {
            anim.SetInteger ("moving", 18);
        }

        if (controller.isGrounded)
        {
            moveDirection=transform.forward * Input.GetAxis ("Vertical") * speed * runSpeed;
            float turn = Input.GetAxis("Horizontal");
            transform.Rotate(0, turn * turnSpeed * Time.deltaTime, 0);
        }
        moveDirection.y -= gravity * Time.deltaTime;
        controller.Move (moveDirection * Time.deltaTime);
    }
}