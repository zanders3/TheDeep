
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

    public void FindAllInRadius(List<Character> list, Vector2 pos, float radius)
    {
        float radiusSq = radius * radius;
        list.Clear();
        for (int i = 0; i<characters.Count; i++)
            if ((characters[i].Position - pos).sqrMagnitude < radiusSq)
                list.Add(characters[i]);
    }

    void Broadphase()
    {
        contacts.Clear();
        for (int i = 0; i<characters.Count; i++)
        {
            for (int j = i+1; j<characters.Count; j++)
            {
                Vector2 d = characters[i].Position - characters[j].Position;
                float len = d.magnitude;
                if (len > Character.Radius)
                    continue;

                contacts.Add(new Contact
                {
                    A = characters[i],
                    B = characters[j],
                    Normal = d * (Character.Radius - len) / len
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

            if (contact.A.OnCollision != null)
                contact.A.OnCollision(contact.B);
            if (contact.B.OnCollision != null)
                contact.B.OnCollision(contact.A);
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

