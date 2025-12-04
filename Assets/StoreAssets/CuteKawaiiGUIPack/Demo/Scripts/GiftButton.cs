// Copyright (C) 2024 ricimi. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at https://unity.com/legal/as-terms.

using UnityEngine;

namespace Ricimi
{
    public class GiftButton : MonoBehaviour
    {
		private Animator animator;

		private void Awake()
		{
			animator = GetComponent<Animator>();
		}

		public void Destroy()
		{
			animator.SetTrigger("Exit");
			Destroy(gameObject, 1.0f);
		}
    }
}