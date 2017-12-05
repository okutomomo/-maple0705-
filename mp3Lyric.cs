﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Xml;

namespace mp3Lyric
{
	/// <summary>
	/// 歌詞取得クラス(j-lyric.net)
	/// </summary>
	public class LyricGetter
	{
		#region プロパティ

		/// <summary>
		/// アーティスト名
		/// </summary>
		public string Artist
		{
			get;
			private set;
		}

		/// <summary>
		/// 曲名
		/// </summary>
		public string Title
		{
			get;
			private set;
		}

		/// <summary>
		/// 歌詞
		/// </summary>
		private string Lyric;

		#endregion

		#region 定数

		/// <summary>
		/// 検索結果から曲のコレクションを得るためのxPath
		/// (あるアーティストのある曲って1つだけだと思うけど、念のため検索結果は全部取得)
		/// </summary>
		private const string searchxPath = "//*[@id=\"mnb\"]/div";

		/// <summary>
		/// 歌詞があるとこ
		/// </summary>
		private const string lyricxPath = "//*[@id=\"Lyric\"]";

		#endregion

		#region コンストラクタ

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="artist">アーティスト名</param>
		/// <param name="title">曲名</param>
		public LyricGetter(string artist, string title)
		{
			Artist = artist;
			Title = title;

			string url;
			url = Search(artist, title);
			if (url != "")
			{
				Lyric = Parse(url);
			}
			else
			{
				Lyric = "取得失敗";
			}
		}

		#endregion

		#region メソッド

		/// <summary>
		/// 歌詞を返します。
		/// </summary>
		/// <returns>歌詞</returns>
		public string GetLyric()
		{

			return Lyric;
		}

		/// <summary>
		/// 与えられたurlのhtmlソースを返します。
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		private string GetHtml(string url)
		{
			WebClient wc = new WebClient();
			Stream stream = wc.OpenRead(url);
			StreamReader sr = new StreamReader(stream, Encoding.UTF8);
			string html = sr.ReadToEnd();   // 曲の検索結果ページのソース

			sr.Close();
			stream.Close();

			return html;
		}

		/// <summary>
		/// 検索用urlを返します。
		/// </summary>
		/// <param name="artist">アーティスト名</param>
		/// <param name="title">曲名</param>
		/// <returns>検索用url</returns>
		private string GetSearchUrl(string artist, string title)
		{

			return "http://search.j-lyric.net/index.php?" + "ka=" + artist + "&kt=" + title;
		}

		/// <summary>
		/// 曲を検索します。
		/// </summary>
		/// <param name="artist">アーティスト名</param>
		/// <param name="title">曲名</param>
		/// <returns>歌詞ページのurl</returns>
		private string Search(string artist, string title)
		{
			HtmlAgilityPack.HtmlNodeCollection searchRslt;
			HtmlAgilityPack.HtmlNode node;
			HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();

			if (artist == "" || title == "")
			{
				Console.WriteLine("! アーティスト名と曲名の両方を指定してください。");
				return "";
			}

			doc.LoadHtml(GetHtml(GetSearchUrl(artist, title)));
			searchRslt = doc.DocumentNode.SelectNodes(searchxPath);
			if (searchRslt == null)
			{
				Console.WriteLine("! 該当する曲は見つかりませんでした。");
				return "";
			}

			// 欲しくない情報が入ってくるので、削除
			searchRslt.RemoveAt(0);

			// searchRslt[0]からが欲しい情報
			int idx = 0;
			node = searchRslt[idx];
			// div[2]のとこはなおしたい
			return node.SelectSingleNode("p[1]").SelectSingleNode("a").GetAttributeValue("href", "");
		}

		/// <summary>
		/// 与えられたurlから、歌詞を取り出します。
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		private string Parse(string url)
		{
			HtmlAgilityPack.HtmlNode htmlLyric;
			HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
			string lyric;

			doc.LoadHtml(GetHtml(url));
			htmlLyric = doc.DocumentNode.SelectSingleNode(lyricxPath);
			lyric = System.Web.HttpUtility.HtmlDecode(htmlLyric.InnerHtml);

			//<br>があればそこで改行
			lyric = lyric.Replace("<br>", "\r\n");

			return lyric;
		}

		#endregion

	}
}