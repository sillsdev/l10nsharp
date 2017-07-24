<?xml version='1.0' ?>
<!-- Convert L10nSharp TMX to XLIFF 1.2, skipping "no longer used" items and preserving the "dynamic" flags. -->
<!-- Copyright (c) 2017 SIL International. -->
<!-- This file is licensed under the MIT license (http://opensource.org/licenses/MIT). -->
<xsl:stylesheet version="1.0" 
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"  xmlns:sil="http://sil.org/software/XLiff">
	<!-- the xmlns:sil namespace declaration is needed for insertion into the output xliff file. -->

	<xsl:output method="xml" encoding="utf-8" indent="yes" xslt:indent-amount="3" 
		xmlns:xslt="http://xml.apache.org/xslt" />

	<!-- set language code variables according to the TMX header attributes -->
	<xsl:variable name="srclang"><xsl:value-of select="/tmx/header/@adminlang"/></xsl:variable>
	<xsl:variable name="targlang"><xsl:value-of select="/tmx/header/@srclang"/></xsl:variable>

	<!-- Write the outer xliff element, including the namespace declarations. -->
	<xsl:template match="/">
		<xsl:element name="xliff" namespace="urn:oasis:names:tc:xliff:document:1.2">
			<!-- This inserts the sil namespace declaration into the xliff element where it is needed. -->
			<xsl:copy-of select="document('')/*/namespace::sil"/>
			<xsl:attribute name="version">1.2</xsl:attribute>
			<xsl:apply-templates/>
		</xsl:element>
	</xsl:template>

	<!-- Transform the tmx and tmx/header elements into a file element, setting all the required attributes. -->
	<!-- Preserve the L10nSharp specific x-hardlinebreak property as an attribute. -->
	<xsl:template match="tmx">
		<xsl:element name="file">
			<xsl:attribute name="original">CHANGE-ME.dll</xsl:attribute>
			<xsl:attribute name="source-language"><xsl:value-of select="$srclang"/></xsl:attribute>
			<xsl:if test="$srclang != $targlang">
				<xsl:attribute name="target-language"><xsl:value-of select="$targlang"/></xsl:attribute>
			</xsl:if>
			<xsl:attribute name="datatype">plaintext</xsl:attribute>
			<xsl:if test="header/prop[@type='x-appversion']">
				<xsl:attribute name="product-version"><xsl:value-of select="header/prop[@type='x-appversion']"/></xsl:attribute>
			</xsl:if>
			<xsl:if test="header/prop[@type='x-hardlinebreakreplacement']">
				<xsl:attribute name="sil:hard-linebreak-replacement"><xsl:value-of select="header/prop[@type='x-hardlinebreakreplacement']"/></xsl:attribute>
			</xsl:if>
			<xsl:apply-templates select="body"/>
		</xsl:element>
	</xsl:template>

	<!-- Retain the body element, transforming its content. -->
	<!-- Omit any translation units that have been marked as "no longer used". -->
	<xsl:template match="body">
		<xsl:element name="body">
			<xsl:apply-templates select="tu[not(prop[@type='x-nolongerused'])]"/>
		</xsl:element>
	</xsl:template>

	<!-- Transform a tu element into a trans-unit element. -->
	<!-- Preserve the L10nSharp specific x-dynamic property as an attribute. -->
	<xsl:template match="tu">
		<xsl:element name="trans-unit">
			<xsl:attribute name="id"><xsl:value-of select="@tuid"/></xsl:attribute>
			<xsl:if test="prop[@type='x-dynamic']">
				<xsl:attribute name="sil:dynamic"><xsl:value-of select="prop[@type='x-dynamic']"/></xsl:attribute>
			</xsl:if>
			<xsl:apply-templates select="tuv"/>
			<xsl:element name="note">ID: <xsl:value-of select="@tuid"/></xsl:element>
			<xsl:apply-templates select="note"/> <!-- note must follow the source and target elements in xliff. -->
		</xsl:element>
	</xsl:template>

	<!-- Transform a tuv element into either a source or target element, depending on its language. -->
	<!-- When $srclang = $targlang, only the source element is written out. -->
	<xsl:template match="tuv">
		<xsl:choose>
			<xsl:when test="@xml:lang = $srclang">
				<xsl:element name="source">
					<xsl:copy-of select="@xml:lang"/>
					<xsl:value-of select="seg"/>
				</xsl:element>
			</xsl:when>
			<xsl:when test="@xml:lang = $targlang">
				<xsl:element name="target">
					<xsl:copy-of select="@xml:lang"/>
					<xsl:value-of select="seg"/>
				</xsl:element>
			</xsl:when>
		</xsl:choose>
	</xsl:template>

	<!-- Copy a note element verbatim. -->
	<xsl:template match="note">
		<xsl:copy-of select="."/>
	</xsl:template>

</xsl:stylesheet>
