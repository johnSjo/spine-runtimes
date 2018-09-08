﻿/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Spine;
using Spine.Unity;

namespace Spine.Unity {
	[CreateAssetMenu(menuName = "Spine/Blend Mode Materials Asset", order = 200)]
	public class BlendModeMaterialsAsset : ScriptableObject {
		public Material multiplyMaterialTemplate;
		public Material screenMaterialTemplate;
		public Material additiveMaterialTemplate;

		public bool applyAdditiveMaterial;

		public void Apply (SkeletonData skeletonData) {
			ApplyMaterials(skeletonData, multiplyMaterialTemplate, screenMaterialTemplate, additiveMaterialTemplate, applyAdditiveMaterial);
		}

		public static void ApplyMaterials (SkeletonData skeletonData, Material multiplyTemplate, Material screenTemplate, Material additiveTemplate, bool includeAdditiveSlots) {
			if (skeletonData == null) throw new ArgumentNullException("skeletonData");

			var atlasPageMaterialCache = new Dictionary<KeyValuePair<AtlasPage, Material>, AtlasPage>();
			var attachmentBuffer = new List<Attachment>();
			var slotsItems = skeletonData.Slots.Items;
			for (int i = 0, slotCount = skeletonData.Slots.Count; i < slotCount; i++) {
				var slot = slotsItems[i];
				if (slot.blendMode == BlendMode.Normal) continue;
				if (!includeAdditiveSlots && slot.blendMode == BlendMode.Additive) continue;

				attachmentBuffer.Clear();
				foreach (var skin in skeletonData.Skins)
					skin.FindAttachmentsForSlot(i, attachmentBuffer);

				Material templateMaterial = null;
				switch (slot.blendMode) {
					case BlendMode.Multiply:
						templateMaterial = multiplyTemplate;
						break;
					case BlendMode.Screen:
						templateMaterial = screenTemplate;
						break;
					case BlendMode.Additive:
						templateMaterial = additiveTemplate;
						break;
				}
				if (templateMaterial == null) continue;

				foreach (var attachment in attachmentBuffer) {
					var renderableAttachment = attachment as IHasRendererObject;
					if (renderableAttachment != null) {
						renderableAttachment.RendererObject = AtlasRegionCloneWithMaterial((AtlasRegion)renderableAttachment.RendererObject, templateMaterial, atlasPageMaterialCache);
					}
				}
			}
			//atlasPageMaterialCache.Clear();
			//attachmentBuffer.Clear();
		}

		static AtlasRegion AtlasRegionCloneWithMaterial (AtlasRegion originalRegion, Material materialTemplate, Dictionary<KeyValuePair<AtlasPage, Material>, AtlasPage> cache) {
			var newRegion = originalRegion.Clone();
			newRegion.page = GetAtlasPageWithMaterial(originalRegion.page, materialTemplate, cache);
			return newRegion;
		}

		static AtlasPage GetAtlasPageWithMaterial (AtlasPage originalPage, Material materialTemplate, Dictionary<KeyValuePair<AtlasPage, Material>, AtlasPage> cache) {
			if (originalPage == null) throw new ArgumentNullException("originalPage");

			AtlasPage newPage = null;
			var key = new KeyValuePair<AtlasPage, Material>(originalPage, materialTemplate);
			cache.TryGetValue(key, out newPage);

			if (newPage == null) {
				newPage = originalPage.Clone();
				var originalMaterial = originalPage.rendererObject as Material;
				newPage.rendererObject = new Material(materialTemplate) {
					name = originalMaterial.name + " " + materialTemplate.name,
					mainTexture = originalMaterial.mainTexture
				};
				cache.Add(key, newPage);
			}

			return newPage;
		}
	
	}

}