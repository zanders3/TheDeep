
using UnityEngine;
using System.Collections.Generic;

public class CharacterUpdater : MonoBehaviour
{
    struct Contact
    {
        public Vector2 Normal;
        public Character A, B;
    }

    List<Character> characters = new List<Character>();
    List<Contact> contacts = new List<Contact>();

    void Broadphase()
    {
        contacts.Clear();
        for (int i = 0; i<characters.Count; i++)
        {
            for (int j = i+1; j<characters.Count; j++)
            {
                Vector2 d = characters[i].Position - characters[j].Position;
                float len = d.magnitude;
                if (len > 0.4f)
                    continue;

                contacts.Add(new Contact
                {
                    A = characters[i],
                    B = characters[j],
                    Normal = d * (0.4f - len) / len
                });
            }
        }
    }

    void Solve()
    {
        for (int i = 0; i<contacts.Count; i++)
        {
            Contact contact = contacts[i];
            contact.A.AddImpulse(contact.Normal);
            contact.B.AddImpulse(-contact.Normal);
        }
    }

    void FixedUpdate()
    {
        GetComponentsInChildren<Character>(characters);

        Broadphase();
        Solve();
        for (int i = 0; i<characters.Count; i++)
            characters[i].DoUpdate();
    }
}

